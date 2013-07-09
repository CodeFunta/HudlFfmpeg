﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hudl.Ffmpeg.Command;
using Hudl.Ffmpeg.Resources.BaseTypes;

namespace Hudl.Ffmpeg.Settings.BaseTypes
{
    /// <summary>
    /// The base implementation of Setting's, will store the basic information on setup, and makes it accessible to parents.
    /// </summary>
    public abstract class BaseSetting : ISetting
    {
        protected BaseSetting(string type)
        {
            Type = type;
        }
        
        /// <summary>
        /// Defines the settings type, name that is given to ffmpeg
        /// </summary>
        public string Type { get; protected set; }

        //public virtual TimeSpan? LengthDifference
        //{
        //    get { return null; }
        //}

        //public virtual TimeSpan? LengthOverride
        //{
        //    get { return null; }
        //}

        public virtual TimeSpan? LengthFromInputs(List<CommandResource<IResource>> resources)
        {
            var totalSeconds = resources.Sum(r => r.Resource.Length.TotalSeconds);
            return totalSeconds > 0d
                       ? (TimeSpan?)TimeSpan.FromSeconds(totalSeconds)
                       : null;
        }
    }
}
