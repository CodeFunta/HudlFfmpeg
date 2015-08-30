using System;
using System.Drawing;
using System.Text;
using Hudl.FFmpeg.Attributes;
using Hudl.FFmpeg.Common;
using Hudl.FFmpeg.Enums;
using Hudl.FFmpeg.Filters.Attributes;
using Hudl.FFmpeg.Filters.BaseTypes;
using Hudl.FFmpeg.Filters.Interfaces;
using Hudl.FFmpeg.Formatters;
using Hudl.FFmpeg.Resources.BaseTypes;

namespace Hudl.FFmpeg.Filters
{
    /// <summary>
    /// Scale Filter, scales the output stream to match the filter settings.
    /// </summary>
    [ForStream(Type = typeof(VideoStream))]
    [Filter(Name = "zoompan", MinInputs = 1, MaxInputs = 1)]
    public class ZoomPan : IFilter
    {
        public ZoomPan(string zoomExpression,int durationInFrames,string xExpression=null,string yExpession=null)
        {
            Zoom = zoomExpression;
            Duration = durationInFrames;
            X = xExpression;
            Y = yExpession;
        }

        [FilterParameter(Name = "z")]
        public string Zoom { get; set; }

        [FilterParameter(Name = "d")]
        [Validate(LogicalOperators.GreaterThan, 0)]
        public int? Duration { get; set; }

        [FilterParameter(Name = "x")]
        public string X { get; set; }

        [FilterParameter(Name = "y")]
        public string Y { get; set; }        
    }
}
