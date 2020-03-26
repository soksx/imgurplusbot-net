using imgurplusbot.bll.Helpers.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace imgurplusbot.bll.Enums
{
    public enum CallbackAction
    {
        ChangeUrl,
        DeleteImage
    }
    public enum LinkRotateType
    {
        [FormatAttribute("[IMG]{0}[/IMG]")]
        BB,
        [FormatAttribute("<img src=\"{0}\" />")]
        HTML,
        [FormatAttribute("[]({0})")]
        TG,
        [FormatAttribute("{0}")]
        TEXT
    }
}
