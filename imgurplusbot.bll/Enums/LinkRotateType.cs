using System;
using System.Collections.Generic;
using System.Text;
using imgurplusbot.bll.Helpers.Attributes;

namespace imgurplusbot.bll.Enums
{
    public enum LinkRotateType
    {
        [Format("[IMG]{0}[/IMG]")]
        BB,
        [Format("<img src=\"{0}\" />")]
        HTML,
        [Format("[ImgurPlustBot]({0})")]
        MARKDOWN,
        [Format("{0}")]
        TEXT
    }
}
