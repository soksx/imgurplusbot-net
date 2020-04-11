using System;
using System.Threading.Tasks;
using imgurplusbot.bll.Helpers;
using imgurplusbot.bll.Interfaces;
using imgurplusbot.bll.Helpers.Attributes;
using imgurplusbot.bll.Helpers.Extensions;
using ImgurModels = imgurplusbot.dal.Models;
using Telegram.Bot.Types;
using TGENUMS = Telegram.Bot.Types.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using imgurplusbot.dal.Helpers;

namespace imgurplusbot.bll.BotHandlers
{
    [Handler(TGENUMS.MessageType.Text, Name = "CommandHandler")]
    public class CommandHandler : BaseHandler
    {
        #region ReadOnly
        private readonly char[] CommandStart = new[] { '!', '@', '/' };
        #endregion
        #region Private Properties
        private ImgurModels.User User { get; set; }
        #endregion
        public CommandHandler(IBotService botService) : base(botService) { }
        public override Task ProcessCallback(ICallbackData callback, Message message, string callbackQueryId) 
        {
            return Task.CompletedTask;
        }

        public override Task ProcessMessage(Message message)
        {
            User = Utils.AddOrGetUser(message.From);
            if (!CommandStart.Contains(message.Text.ToCharArray()[0]))
                return Task.CompletedTask;

            return Task.WhenAll(Commands(message));
        }
        #region Private Methods
        private IEnumerable<Task> Commands(Message message)
        {
            string[] msgCommand = message.Text.Split(" ");
            var pato = msgCommand[1..];
            return this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where((met) => msgCommand[0].TrimStart(CommandStart).Equals(met.GetAttributeValue((Command com) => com.Name), StringComparison.InvariantCultureIgnoreCase) && (met.GetAttributeValue((Command com) => com.Format) == null && msgCommand.Length == 1 || (met.GetAttributeValue((Command com) => com.Format)?.Split(" ").Length == (msgCommand.Length - 1))) && (met.GetAttributeValue((Command com) => com.AdminOnly) == User.IsAdmin || User.IsAdmin))
                .Select((met) => (Task)met.Invoke(this, new object[] { message, met.GetAttributeValue((Command com) => com.Format), msgCommand[1..] }));
        }
        #endregion
        #region Commands
        [Command("start")]
        private async Task StartCommand(Message message, string format, params string[] commandParams)
        {

            string msg = "<b>Welcome!</b>" + Environment.NewLine + Environment.NewLine +
                "Use this bot to upload images, stickers and gifs directly to Imgur." + Environment.NewLine + Environment.NewLine +
                "<b>•</b> Send any image as documment or just as simple image and the bot will upload it to imgur, giving the direct link to the image. <i>The format of the image depends from the original file.</i>" + Environment.NewLine + Environment.NewLine +
                "<b>•</b> Send any Telegram gif and the bot will convert it and upload to imgur, giving the direct link. <i>The gif format always will be .mp4, but you will have the possibility to convert in .gif.</i>" + Environment.NewLine + Environment.NewLine +
                "<b>• Important: the file limit is about 10 MB (200 MB if its a video)</b>";
            await Bot.SendTextMessageAsync(message.Chat.Id, msg, TGENUMS.ParseMode.Html);
        }
        [Command("stats")]
        private async Task CurrentUserStats(Message message, string format, params string[] commandParams)
        {
            string template = "<b>User Stats:</b>" + Environment.NewLine + Environment.NewLine +
                "<b>•</b> Total uploads: {0}" + Environment.NewLine +
                "<b>•</b> Succesfully uploads: {1}" + Environment.NewLine +
                "<b>•</b> Deleted uploads: {2}";
            var userTotalUploads = DatabaseContext.UserUploads.Where((uu) => uu.UserId == User.Id);
            await Bot.SendTextMessageAsync(message.Chat.Id, string.Format(template, userTotalUploads.Count(), userTotalUploads.Count() - userTotalUploads.Where((uu) => uu.ImgurLink == null).Count(), userTotalUploads.Where((uu) => uu.DeleteDate.HasValue).Count()), TGENUMS.ParseMode.Html);
        }
        [Command("stats", "{0}", true)]
        private async Task UserStats(Message message, string format, params string[] commandParams)
        {
            string template = "<b>{0} Stats:</b>" + Environment.NewLine + Environment.NewLine +
                "<b>•</b> Total uploads: {1}" + Environment.NewLine +
                "<b>•</b> Succesfully uploads: {2}" + Environment.NewLine +
                "<b>•</b> Deleted uploads: {3}";
            ImgurModels.User lUser = DbUtils.GetUser((u) => u.UserName.Equals(commandParams[0], StringComparison.InvariantCultureIgnoreCase) || u.TgId.ToString().Equals(commandParams[0], StringComparison.InvariantCultureIgnoreCase));
            if (lUser == null)
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, string.Format("No stats found for user: {0}", commandParams[0]), TGENUMS.ParseMode.Default);
                return;
            }
            var userTotalUploads = DatabaseContext.UserUploads.Where((uu) => uu.UserId == lUser.Id);
            await Bot.SendTextMessageAsync(message.Chat.Id, string.Format(template, commandParams[0], userTotalUploads.Count(), userTotalUploads.Count() - userTotalUploads.Where((uu) => uu.ImgurLink == null).Count(), userTotalUploads.Where((uu) => uu.DeleteDate.HasValue).Count(), TGENUMS.ParseMode.Html));
        }
        [Command("globalstats", null, true)]
        private async Task GlobalStats(Message message, string format, params string[] commandParams)
        {
            string template = "<b>Global Stats:</b>" + Environment.NewLine + Environment.NewLine +
               "<b>•</b> Total uploads: {0}" + Environment.NewLine +
               "<b>•</b> Succesfully uploads: {1}" + Environment.NewLine +
               "<b>•</b> Deleted uploads: {2}";
            int totalUploads = DatabaseContext.UserUploads.Count();
            int failedUploads = DatabaseContext.UserUploads.Where((uu) => uu.ImgurLink == null).Count();
            int deletedUploads = DatabaseContext.UserUploads.Where((uu) => uu.DeleteDate.HasValue).Count();
            await Bot.SendTextMessageAsync(message.Chat.Id, string.Format(template, totalUploads, totalUploads - failedUploads, deletedUploads), TGENUMS.ParseMode.Html);

        }
        #endregion
    }
}
