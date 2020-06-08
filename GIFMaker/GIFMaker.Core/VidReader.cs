using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace GIFMaker.Core
{
    unsafe class VidReader : IDisposable
    {
        private readonly AVFormatContext* _pFormatCtx;
        private readonly AVCodecContext* _pVCtx;
        private readonly AVFrame* _pVFrame;
        private readonly AVPacket* _packet;
        private readonly SwsContext* _pSwsCtx;
        private readonly int _VSI;

        public int width { get; }
        public int height { get; }

        public unsafe VidReader(string videoPath)
        {
            // C#에서의 제한으로 _pFormatCtx의 포인터를 직접 건내줄 수 없음
            AVFormatContext* temp;
            int ret = ffmpeg.avformat_open_input(&temp, videoPath, null, null);
            _pFormatCtx = temp;

            //미디어 파일 열기 file주소 or URL
            //파일의 헤더로 부터 파일 포맷에 대한 정보를 읽어낸 뒤 AVFormatContext에 저장한다.
            //그 뒤의 인자들은 각각 Input Source (스트리밍 URL이나 파일경로), Input Format, demuxer의 추가옵션 이다.
            if (ret != 0)
            {
                throw new FFMpegException("File Open Failed");
            }

            ret = ffmpeg.avformat_find_stream_info(_pFormatCtx, null); //열어놓은 미디어 파일의 스트림 정보를 가져온다
            if (ret < 0)
            {
                throw new FFMpegException("Fail to get Stream Inform");
            }

            _VSI = ffmpeg.av_find_best_stream(_pFormatCtx, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, null, 0);//비디오 스트림 정보를 가져온다.

            // dump_format은 포맷 정보 출력에 이용. 일단은 필요없다
            //ffmpeg.av_dump_format(_pFormatCtx, 0, "", 0);

            /*codec_id에 맞는 비디오 디코더를 찾는다*/
            AVCodec* pVideoCodec = ffmpeg.avcodec_find_decoder(_pFormatCtx->streams[_VSI]->codecpar->codec_id);
            if (pVideoCodec == null)
            {
                throw new FFMpegException("No Decoder");
            }

            /*디코더 메모리 할당*/
            _pVCtx = ffmpeg.avcodec_alloc_context3(pVideoCodec);

            if (ffmpeg.avcodec_parameters_to_context(_pVCtx, _pFormatCtx->streams[_VSI]->codecpar) < 0)
            {
                throw new FFMpegException("avcodec_parameters_to_context Error");
            }

            /*
            *디코더 정보를 찾을 수 있다면 AVContext에 그 정보를 넘겨줘서 Decoder를 초기화 함
             *세번째 인자 : 디코더 초기화에 필요한 추가 옵션. 비트레이트 정보나 스레트 사용여부를 정해줄 수 있다.
            */
            if (ffmpeg.avcodec_open2(_pVCtx, pVideoCodec, null) < 0)
            {
                throw new FFMpegException("Fail to Initialize Decoder");
            }

            _pVFrame = ffmpeg.av_frame_alloc(); //alloc 않하면 밑에서 프레임을 받아올수 없다.
            _packet = ffmpeg.av_packet_alloc(); //패킷 데이터

            _pSwsCtx = ffmpeg.sws_getContext(_pVCtx->width, _pVCtx->height, _pVCtx->pix_fmt,
                _pVCtx->width, _pVCtx->height, AVPixelFormat.AV_PIX_FMT_RGB24,
                ffmpeg.SWS_LANCZOS, null, null, null);

            width = _pVCtx->width;
            height = _pVCtx->height;
        }

        // pos의 단위는 ms
        public byte[] ByteOfPos(long pos)
        {
            ffmpeg.avcodec_flush_buffers(_pVCtx);
            ffmpeg.av_seek_frame(_pFormatCtx, -1, _pFormatCtx->start_time + pos * 1000, ffmpeg.AVSEEK_FLAG_BACKWARD);

            byte[] data = new byte[3 * _pVCtx->width * _pVCtx->height];
            int[] outLinesize = { 3 * _pVCtx->width };

            int ret;
            double prevPts = 0;
            bool done = false;

            while (ffmpeg.av_read_frame(_pFormatCtx, _packet) == 0 && (!done))
            {
                if (_packet->stream_index == _VSI)
                { //패킷이 비디오 패킷이면...
                    if ((ret = ffmpeg.avcodec_send_packet(_pVCtx, _packet)) != 0)
                    {
                        ffmpeg.av_log(null, ffmpeg.AV_LOG_ERROR, "avcodec_send_packet failed " + ret + " " + ffmpeg.AVERROR(ffmpeg.EINVAL) + " " + ffmpeg.AVERROR(ffmpeg.ENOMEM) + "\n");
                    }
                    while (ffmpeg.avcodec_receive_frame(_pVCtx, _pVFrame) >= 0)
                    {
                        // 디코딩에 성공하면 pVframe을 가져와 사용한다.

                        double pts = (_pVFrame->best_effort_timestamp - _pFormatCtx->start_time);
                        if (ffmpeg.av_q2d(_pVCtx->time_base) > 0.01)
                            pts *= ffmpeg.av_q2d(_pVCtx->time_base) * 2;

                        // pts가 해당 위치를 넘겼다면 이제 시간에 맞는 정확한 프레임이 데이터 속에 존재할 것이다
                        if (pts > pos)
                        {
                            done = true;
                            break;
                        }
                        else
                        {
                            double frame_delay = (1 + _pVFrame->repeat_pict) * (pts - prevPts);

                            // frame_delay는 현재 pts와 이전 pts를 기반으로 계산된다
                            // 따라서 프레임이 가변적인 영상의 경우 계산에 오차가 생길 가능성이 존재한다.
                            // 이런 경우를 대비해 frame_delay에 2를 곱해 조금 전부터 미리 프레임을 복사해둔다
                            if (prevPts == 0 || pts + frame_delay * 2 >= pos)
                            {
                                fixed (byte* pData = data)
                                {
                                    byte*[] outData = { pData };
                                    ffmpeg.sws_scale(_pSwsCtx, _pVFrame->data, _pVFrame->linesize, 0, _pVCtx->height, outData, outLinesize);
                                }
                            }
                        }

                        prevPts = pts;
                    }

                }

                ffmpeg.av_packet_unref(_packet);
            }

            return data;
        }

        // 모든 단위는 ms
        public List<byte[]> BytesOfPos(long start, long end, long delay)
        {
            ffmpeg.avcodec_flush_buffers(_pVCtx);
            ffmpeg.av_seek_frame(_pFormatCtx, -1, start * 1000, ffmpeg.AVSEEK_FLAG_BACKWARD);

            List<byte[]> datas = new List<byte[]>();
            datas.Add(new byte[3 * _pVCtx->width * _pVCtx->height]);
            int[] outLinesize = { 3 * _pVCtx->width };

            int ret;
            double pos = start;
            double prevPts = 0;
            bool done = false;

            while (ffmpeg.av_read_frame(_pFormatCtx, _packet) == 0 && (!done))
            {
                if (_packet->stream_index == _VSI)
                { //패킷이 비디오 패킷이면...
                    if ((ret = ffmpeg.avcodec_send_packet(_pVCtx, _packet)) != 0)
                    {
                        ffmpeg.av_log(null, ffmpeg.AV_LOG_ERROR, "avcodec_send_packet failed " + ret + " " + ffmpeg.AVERROR(ffmpeg.EINVAL) + " " + ffmpeg.AVERROR(ffmpeg.ENOMEM) + "\n");
                    }
                    while (ffmpeg.avcodec_receive_frame(_pVCtx, _pVFrame) >= 0)
                    {
                        // 디코딩에 성공하면 pVframe을 가져와 사용한다.

                        double pts = (_pVFrame->best_effort_timestamp - _pFormatCtx->start_time);
                        if (ffmpeg.av_q2d(_pVCtx->time_base) > 0.01)
                            pts *= ffmpeg.av_q2d(_pVCtx->time_base) * 2;

                        // pts가 해당 위치를 넘겼다면 이제 시간에 맞는 정확한 프레임이 데이터 속에 존재할 것이다
                        if (pts > pos)
                        {
                            pos += delay;
                            if (pos >= end)
                            {
                                done = true;
                                break;
                            }
                            else
                            {
                                datas.Add(datas.Last().ToArray());
                            }
                        }
                        else
                        {
                            double frame_delay = (1 + _pVFrame->repeat_pict) * (pts - prevPts);

                            // frame_delay는 현재 pts와 이전 pts를 기반으로 계산된다
                            // 따라서 프레임이 가변적인 영상의 경우 계산에 오차가 생길 가능성이 존재한다.
                            // 이런 경우를 대비해 frame_delay에 2를 곱해 조금 전부터 미리 프레임을 복사해둔다
                            if (prevPts == 0 || pts + frame_delay * 2 >= pos)
                            {
                                fixed (byte* pData = datas.Last())
                                {
                                    byte*[] outData = { pData };
                                    ffmpeg.sws_scale(_pSwsCtx, _pVFrame->data, _pVFrame->linesize, 0, _pVCtx->height, outData, outLinesize);
                                }
                            }
                        }

                        prevPts = pts;
                    }

                }

                ffmpeg.av_packet_unref(_packet);
            }

            return datas;
        }

        //Image Resize Helper Method
        private static Bitmap ResizeImage(String filename, int maxWidth, int maxHeight)
        {
            using (Image originalImage = Image.FromFile(filename))
            {
                //Caluate new Size
                int newWidth = originalImage.Width;
                int newHeight = originalImage.Height;
                double aspectRatio = (double)originalImage.Width / (double)originalImage.Height;

                if (aspectRatio <= 1 && originalImage.Width > maxWidth)
                {
                    newWidth = maxWidth;
                    newHeight = (int)Math.Round(newWidth / aspectRatio);
                }
                else if (aspectRatio > 1 && originalImage.Height > maxHeight)
                {
                    newHeight = maxHeight;
                    newWidth = (int)Math.Round(newHeight * aspectRatio);
                }

                Bitmap newImage = new Bitmap(newWidth, newHeight);

                using (Graphics g = Graphics.FromImage(newImage))
                {
                    //--Quality Settings Adjust to fit your application
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.DrawImage(originalImage, 0, 0, newImage.Width, newImage.Height);
                    return newImage;
                }
            }
        }


        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null

                var pFormatCtx = _pFormatCtx;
                ffmpeg.avformat_close_input(&pFormatCtx);

                var pVCtx = _pVCtx;
                ffmpeg.avcodec_free_context(&pVCtx);

                var pVFrame = _pVFrame;
                ffmpeg.av_frame_free(&pVFrame);

                ffmpeg.sws_freeContext(_pSwsCtx);

                var packet = _packet;
                ffmpeg.av_packet_free(&packet);

                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~VidReader()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class FFMpegException : Exception
    {
        public FFMpegException(string message)
            : base(message)
        { }
    }
}
