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
using Hudl.FFmpeg.Metadata;
using FinalYearProject_11010841;
using System.Globalization;

namespace TestHudlFfmpeg
{
    class Program
    {
        static void Main(string[] args)
        {


            var culture = CultureInfo.CreateSpecificCulture("en-US");

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            
            
            
            var outputPath = "C:/temp/Media/KLApp/renderout";
            var FFmpegPath = "C:/temp/Media/KLApp/ffmpeg/FFmpeg.exe";
            var FFprobePath = "C:/temp/Media/KLApp/ffmpeg/FFprobe.exe";
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
                    //new DurationOutput(20),
                    new FrameRate(24),
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
                        //new DurationOutput(5),
                        new OverwriteOutput()
                    );
                #region Test
                var fps = 24;
                var BPM = 125;
                var TS = 8;
                var bps = BPM / 60;
                var TICKS = (fps / bps) - 1;

                
                var clrFlt = new Color();
                //flt.Duration = track.Info.AudioMetadata.Duration;
                clrFlt.ColorName = "blue";
                clrFlt.FrameRate = fps;
                clrFlt.Size = new System.Drawing.Size(1280, 720);

                /*
                var overlay = new OverlayEx();
                overlay.X = "W/2-w/2";
                overlay.Y = "H/2-h/2";
                overlay.Format = OverlayVideoFormatType.Rgb;
                

                //var imgSet = SettingsCollection.ForInput(new DurationInput(5));
                var img = Resource.From(Path.Combine(outputPath, "speaker.png")).LoadMetadata().Streams.OfType<VideoStream>().FirstOrDefault();
                var pulsate = prepCommand
                                        .WithInput<VideoStream>(Path.Combine(outputPath, "speaker.png"))
                                        .Filter(new Pulsate(TimeSpan.FromSeconds(2), fps, Path.Combine(outputPath, "bg.png")))
                                        //.Filter(Filterchain.FilterTo<VideoStream>(new Scale(Convert.ToInt32(img.Info.VideoMetadata.Width * 1), Convert.ToInt32(img.Info.VideoMetadata.Height * 1))))
                                        //.MapTo<Mp4>(Path.Combine(outputPath, "test.mp4"), outputSettings)
                                        ;



                var main = prepCommand
                                        .Filter(Filterchain.FilterTo<VideoStream>(clrFlt))
                                        .Select(pulsate.StreamIdentifiers)
                                        .Filter(Filterchain.FilterTo<VideoStream>(overlay))
                                        .MapTo<Mp4>(Path.Combine(outputPath, "test.mp4"), outputSettings)
                                        ;

                prepCommand.Render();
                return;
                */

                var strack = Path.Combine(outputPath, "track.mp3");
                var track = Resource.From(strack).LoadMetadata().Streams.OfType<AudioStream>().FirstOrDefault();
                
                
              
                //const int PIXELS_SECOND = 1500;
                const float THRESHOLD_WINDOW = 0.5f;
                const float ONSET_SENSITIVITY = 0.8f;
                

                var audioAnalysis = new AudioAnalysis();
                audioAnalysis.LoadAudioFromFile(strack);
                //Find onsets
                List<TimeSpan> lstOnsets = audioAnalysis.GetNormilizedOnsets(TimeSpan.FromSeconds(3), ONSET_SENSITIVITY, THRESHOLD_WINDOW);

                StringBuilder sb = new StringBuilder();
                bool bFirst = true;
                CommandStage prevOutput = null;
                              

                for (int itemNo = 0; itemNo < lstOnsets.Count; itemNo++)
                {
                    // Place the note in accordance to its position in the song
                        double xPosition = lstOnsets[itemNo].TotalSeconds;
                        
                        
                        if(bFirst)
                        {
                            bFirst = false;
                        }
                        else
                        {
                            sb.Append("+");
                        }
                        sb.AppendFormat("between(t,{0},{1})", xPosition.ToString("F1", CultureInfo.InvariantCulture), (xPosition + 0.100).ToString("F1", CultureInfo.InvariantCulture));
                        //sb.AppendFormat("eq(t,{0})", xPosition.ToString("F0", CultureInfo.InvariantCulture));                     
                }

                //test wav convert
                //var audioOut = prepCommand
                //    .WithInput<AudioStream>(Path.Combine(outputPath, "track.mp3"))
                //    .MapTo<Wav>(Path.Combine(outputPath, "track.wav"), audioOutSettings)
                //    //.MapTo<Mp3>(Path.Combine(outputPath, "ocean-short.mp3"), audioOutSettings)
                //    ;

                var sOutPath = Path.Combine(outputPath, "test.mp4");

                var ov = new OverlayEx();
                ov.X = "W/2-w/2";
                ov.Y = "H/2-h/2";
                ov.Enable = string.Format("'{0}'",sb.ToString());
                //ov.Enable = string.Format("{0}", sb.ToString());

                //prevOutput = prepCommand
                //        .Filter(Filterchain.FilterTo<VideoStream>(clrFlt))
                //        .WithInput<VideoStream>(Path.Combine(outputPath, "speaker.png"))
                //        .Filter(Filterchain.FilterTo<VideoStream>(ov))
                //        ;
                prepCommand
                          .Filter(Filterchain.FilterTo<VideoStream>(clrFlt))
                          .WithInput<VideoStream>(Path.Combine(outputPath, "speaker.png"))
                          .Filter(Filterchain.FilterTo<VideoStream>(ov))
                          .WithInput<AudioStream>(strack)
                          .MapTo<Mp4>(sOutPath, outputSettings);
                                
                prepCommand.Render();
                return;
                


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
              
                #endregion
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
                var e = ex.InnerException as FFmpegProcessingException;
                if (e != null)
                {
                    Console.WriteLine(e.ErrorOutput);
                }
                else if(ex.InnerException!=null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
                else
                {
                    Console.WriteLine(ex.Message);
                }
                
            }
            finally
            {
                commandFactory_prep.GetOutputs().ForEach(temp =>
                {
                    //File.Delete(temp.FullName);
                });
            }
            
        }
    }
}
