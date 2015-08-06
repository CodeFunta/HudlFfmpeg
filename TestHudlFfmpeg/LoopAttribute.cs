using Hudl.FFmpeg.Attributes;
using Hudl.FFmpeg.Enums;
using Hudl.FFmpeg.Resources.BaseTypes;
using Hudl.FFmpeg.Settings.Attributes;
using Hudl.FFmpeg.Settings.Interfaces;
using Hudl.FFmpeg.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hudl.FFmpeg.Settings
{
    /// <summary>
    /// Set frame rate (Hz value, fraction or abbreviation).
    /// </summary>
    [ForStream(Type = typeof(VideoStream))]
    [Setting(Name = "loop",IsPreDeclaration = true, ResourceType = SettingsCollectionResourceType.Input)]
    public class Loop : ISetting
    {
        public Loop()
        {
        }
        public Loop(int loop)
        {
            if (loop <= 0)
            {
                throw new ArgumentException("Loop value must be greater than zero.");
            }

            Value = loop;
        }

        [SettingParameter]
        [Validate(LogicalOperators.GreaterThanOrEqual, 1)]
        public int Value { get; set; }
    }

    /// <summary>
    /// Set frame rate (Hz value, fraction or abbreviation).
    /// </summary>
    [ForStream(Type = typeof(VideoStream))]
    [Setting(Name = "framerate", IsPreDeclaration = true, ResourceType = SettingsCollectionResourceType.Input)]
    public class FrameRateIn : ISetting
    {
        public FrameRateIn()
        {
        }
        public FrameRateIn(string framerate)
        {
          
            Rate = framerate;
        }

        [SettingParameter]
        [Validate(typeof(NullOrWhitespaceValidator))]
        public string Rate { get; set; }
    }
}
