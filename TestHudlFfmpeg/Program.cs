using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hudl.FFmpeg;
using Hudl.FFmpeg.Command;
using System.IO;
using Hudl.FFmpeg.Resources.BaseTypes;
using Hudl.FFmpeg.Sugar;
using Hudl.FFmpeg.Filters.BaseTypes;
using Hudl.FFmpeg.Filters;
using Hudl.FFmpeg.Settings.BaseTypes;
using Hudl.FFmpeg.Settings;
using Hudl.FFmpeg.Resources;
using Hudl.FFmpeg.Exceptions;
using Hudl.FFmpeg.DataTypes;
using Hudl.FFmpeg.Filters.Templates;
using Hudl.FFmpeg.Enums;

namespace TestHudlFfmpeg
{
    class Program
    {
        static void Main(string[] args)
        {
            var outputPath = "C:/temp/renderout";
            var FFmpegPath = "C:/temp/ffmpeg/FFmpeg.exe";
            var FFprobePath = "C:/temp/ffmpeg/FFprobe.exe";
            ResourceManagement.CommandConfiguration = CommandConfiguration.Create(outputPath, FFmpegPath, FFprobePath);
            var commandFactory_prep = CommandFactory.Create();
            var commandFactory = CommandFactory.Create();
            try
            {
               
                var imgSet = SettingsCollection.ForInput(new DurationInput(5));
                

                var outputPrepSettings = SettingsCollection.ForOutput(
                    new CodecVideo("libx264"),
                    new PixelFormat(Hudl.FFmpeg.Enums.PixelFormatType.Yuv420P),
                    //new DurationOutput(5),
                    new FrameRate(30),
                    new OverwriteOutput());

                var outputSettings = SettingsCollection.ForOutput(
                    new CodecVideo("libx264"),
                    new PixelFormat(Hudl.FFmpeg.Enums.PixelFormatType.Yuv420P),
                    //new DurationOutput(5),
                    new FrameRate(30),
                    //new Size( Hudl.FFmpeg.Enums.ScalePresetType.Hd720),
                    //new RemoveAudio(),
                    new TrimShortest(),
                    //new BitRateAudio(125),
                    //new CodecAudio(AudioCodecType.ExperimentalAac),
                    new OverwriteOutput());

                var prepCommand = commandFactory_prep.CreateResourceCommand();
                
                var filterConcat = Filterchain.FilterTo<VideoStream>(new Concat());
                

                //imgCommand
                //    .WithInput<VideoStream>(Path.Combine(outputPath, "sailing_over_indian_ocean-wide.jpg"), SettingsCollection.ForInput(new FrameRateIn("1/5")))
                //    .WithInput<VideoStream>(Path.Combine(outputPath, "v1.mp4"))
                //    .FilterEach(Filterchain.FilterTo<VideoStream>(new Scale(Hudl.FFmpeg.Enums.ScalePresetType.Hd720), new SetSar(Ratio.Create(1, 1))))
                //    //.MapTo<Mp4>(outputSettings)
                //    ;

                var audioOutSettings = SettingsCollection.ForOutput(
                        new DurationOutput(5),
                        new OverwriteOutput()
                    );
                
                //var audioOut = prepCommand
                //    .WithInput<AudioStream>(Path.Combine(outputPath, "ocean.mp3"))
                //    .MapTo<Mp3>(Path.Combine(outputPath, "ocean-short.mp3"),audioOutSettings)
                //    ;

                //prepCommand
                //   .WithInput<AudioStream>(Path.Combine(outputPath, "Ocean.mp3"))
                //   .Filter(Filterchain.FilterTo<AudioStream>(
                //        new AFade(2, 19 - 2, AudioUnitType.Seconds, FadeTransitionType.Out)
                //    ))
                //    .WithInput<VideoStream>(Path.Combine(outputPath, "t.mp4"))
                //   .MapTo<Mp4>(Path.Combine(outputPath, "test.mp4"), outputSettings)
                //   ;
                //prepCommand.Render();
                //return;

                var imgOut = prepCommand
                    .WithInput<VideoStream>(Path.Combine(outputPath, "sailing_over_indian_ocean-wide.jpg"), SettingsCollection.ForInput(new FrameRateIn("1/5")))
                    //.WithInput<VideoStream>(Path.Combine(outputPath, "v1.mp4"))
                    .Filter(Filterchain.FilterTo<VideoStream>(new Scale(Hudl.FFmpeg.Enums.ScalePresetType.Hd720), new SetSar(Ratio.Create(1, 1))))
                    .MapTo<Mp4>(outputPrepSettings)
                    ;

                var vidOut = prepCommand
                    //.WithInput<VideoStream>(Path.Combine(outputPath, "sailing_over_indian_ocean-wide.jpg"), SettingsCollection.ForInput(new FrameRateIn("1/5")))
                    .WithInput<VideoStream>(Path.Combine(outputPath, "v1.mp4"))
                    .Filter(Filterchain.FilterTo<VideoStream>(new Scale(Hudl.FFmpeg.Enums.ScalePresetType.Hd720), new SetSar(Ratio.Create(1, 1))))
                    .MapTo<Mp4>(outputPrepSettings)
                    ;

                commandFactory_prep.Render();
                

                

                //get total length
                TimeSpan videoLength = TimeSpan.FromSeconds(0);
                double disolveDuration = 3;
                commandFactory_prep.GetOutputs().ForEach(o =>
                {
                    var stream = Resource.From(o.FullName).LoadMetadata().Streams.OfType<VideoStream>().FirstOrDefault();
                    
                    if (stream != null && stream.Info.VideoMetadata!=null)
                        videoLength += stream.Info.VideoMetadata.Duration;

                });
                videoLength -= TimeSpan.FromSeconds(disolveDuration);

                var audioFadeOutFilterChain = Filterchain.FilterTo<AudioStream>(
                        new AFade(2, (int)videoLength.TotalSeconds - 2, AudioUnitType.Seconds, FadeTransitionType.Out)
                    );

                var disolveCommand = commandFactory_prep.CreateResourceCommand();

                var disolved = disolveCommand
                    .WithInput<VideoStream>(imgOut.First().OutputName)
                    .WithInput<VideoStream>(vidOut.First().OutputName)
                    .Filter(new Dissolve(disolveDuration))
                    .MapTo<Mp4>(outputPrepSettings)
                ;

                disolveCommand.Render();

                var outCommand = commandFactory.CreateOutputCommand();

                var finalVideo = outCommand
                    .WithInput<AudioStream>(Path.Combine(outputPath, "Ocean.mp3"))
                    .Filter(audioFadeOutFilterChain)
                    .WithInput<VideoStream>(disolved.First().OutputName)
                    .MapTo<Mp4>(Path.Combine(outputPath, "vConcat.mp4"), outputSettings)
                    ;

                //var finalVideo = outCommand
                //    .WithInput<AudioStream>(Path.Combine(outputPath, "Ocean.mp3"))
                //    .Filter(audioFadeOutFilterChain)
                //    .MapTo<Mp4>(Path.Combine(outputPath, "vConcat.mp4"), outputSettings)
                //    ;

                //outCommand
                //    //.Select(disolvedVideo.First().GetStreamIdentifiers()
                //    .WithInput<AudioStream>(Path.Combine(outputPath, "ocean-wave-2.mp3"))
                //    .Filter(filterConcat)
                //    .MapTo<Mp4>(Path.Combine(outputPath, "vConcat.mp4"), outputSettings)
                //    ;

                commandFactory.Render();
               
                //commandFactory_prep.GetOutputs().First()
            }
            catch (FFmpegRenderingException ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(new String('*',100));
                
                Console.Write((ex.InnerException as FFmpegProcessingException).ErrorOutput);
                
            }
            finally
            {
                commandFactory_prep.GetOutputs().ForEach(temp =>
                {
                    File.Delete(temp.FullName);
                });
            }
            
        }
    }
}
