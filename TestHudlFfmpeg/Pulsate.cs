using System;
using System.Collections.Generic;
using Hudl.FFmpeg.Command;
using Hudl.FFmpeg.Enums;
using Hudl.FFmpeg.Filters.BaseTypes;
using Hudl.FFmpeg.Metadata;
using Hudl.FFmpeg.Resources.BaseTypes;
using Hudl.FFmpeg.Sugar;
using Hudl.FFmpeg.Settings.BaseTypes;
using Hudl.FFmpeg.Settings;

namespace Hudl.FFmpeg.Filters.Templates
{
    public class Pulsate : FilterchainTemplate
    {
        

        public Pulsate(TimeSpan pulsateDuration,int frameRate,string backgroundImagePath)
        {
            Duration = pulsateDuration;
            FrameRate = frameRate;
            BackgroundImagePath = backgroundImagePath;
        }
        public Pulsate(double pulsateDuration, int frameRate, string backgroundImagePath)
        {
            Duration = TimeSpan.FromSeconds(pulsateDuration);
            FrameRate = frameRate;
            BackgroundImagePath = backgroundImagePath;
        }

        private TimeSpan Duration { get; set; }
        private int FrameRate { get; set; }

        private string BackgroundImagePath { get; set; }

        public override List<StreamIdentifier> SetupTemplate(FFmpegCommand command, List<StreamIdentifier> streamIdList)
        {
            if (streamIdList.Count > 1)
            {
                throw new InvalidOperationException("Pulsate requires one input video stream.");
            }
            if (FrameRate <=0)
            {
                throw new InvalidOperationException("Pulsate requires frame rate > 0.");
            }

            var streamFrom = streamIdList[0];

                                    
            //output ==
            // - (zoom-in, zoom-out)


            int zoomFrameDuration = Convert.ToInt32(Duration.TotalSeconds * 0.5 * FrameRate); // it's a half for zoom-in and another half for zoom-out
            //var zoomFrameDuration = 100;

            var streamSplit = command.Select(streamFrom)
                //.WithInput<VideoStream>(BackgroundImagePath,SettingsCollection.ForInput(new DurationInput(Duration)))
                //.Filter(new TrimVideo(null, Duration.TotalSeconds * 0.5, VideoUnitType.Seconds))
                //.Filter(Filterchain.FilterTo<VideoStream>(new Fps(FrameRate),new Scale(Convert.ToInt32(meta.Width * 1.3), Convert.ToInt32(meta.Height * 1.3))))
                                   .Filter(new TrimVideo(null, Duration.TotalSeconds * 0.5, VideoUnitType.Seconds))

                                   .Filter(Filterchain.FilterTo<VideoStream>(new Split(2)));


            var zoomIn = streamSplit.Take(0)
                                    .Filter(Filterchain.FilterTo<VideoStream>(new ZoomPan("'min(zoom+0.1,1.2)'", zoomFrameDuration, "'iw/2-(iw/zoom/2)'", "'ih/2-(ih/zoom/2)'")))
                                    ;

            var zoomOut = streamSplit.Take(1)
                                    .Filter(Filterchain.FilterTo<VideoStream>(new ZoomPan("'if(lte(zoom,1.0),1.2,max(1.001,zoom-0.1))'", zoomFrameDuration, "'iw/2-(iw/zoom/2)'", "'ih/2-(ih/zoom/2)'")))
                                    ;

            var streamFromMetadata = MetadataHelpers.GetMetadataInfo(command, streamFrom);
            var meta = streamFromMetadata.VideoStream.VideoMetadata;

            var zoom = command
                                .Select(zoomIn.StreamIdentifiers)
                                .Select(zoomOut.StreamIdentifiers)
                                .Filter(Filterchain.FilterTo<VideoStream>(new Concat()))
                                .Filter(Filterchain.FilterTo<VideoStream>(new Scale(meta.Width, meta.Height)))
                                .Filter(new TrimVideo(null, Duration.TotalSeconds, VideoUnitType.Seconds))
                                ;
            
            
           
            //var nsrc = new Color();
            ////nsrc.Duration = Duration;
            //nsrc.FrameRate = FrameRate;
            //nsrc.Size = new System.Drawing.Size(Convert.ToInt32(meta.Width * 1.25), Convert.ToInt32(meta.Height * 1.25));

            var overlay = new OverlayEx();
            overlay.X = "W/2-w/2";
            overlay.Y = "H/2-h/2";
            overlay.Format = OverlayVideoFormatType.Rgb;

            var result = command
                                //.WithInput<VideoStream>(BackgroundImagePath,SettingsCollection.ForInput(new DurationInput(Duration)))
                                //.Filter(new TrimVideo(null, Duration.TotalSeconds * 0.5, VideoUnitType.Seconds))
                                //.Filter(Filterchain.FilterTo<VideoStream>(new Fps(FrameRate),new Scale(Convert.ToInt32(meta.Width * 1.3), Convert.ToInt32(meta.Height * 1.3))))
                                .Select(zoom.StreamIdentifiers)
                                //.Filter(Filterchain.FilterTo<VideoStream>(overlay))
                                ;

            return result.StreamIdentifiers;
        }
    }
}
