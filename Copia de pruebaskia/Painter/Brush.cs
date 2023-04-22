using System;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using SkiaSharp;

namespace pruebaskia.Painter
{
	public class Brush
    {
        private List<SKPoint> _points = new List<SKPoint>();

        public virtual float Stabilization { get; set; }
        protected SKPath _Currentpath { get; set; }

        public virtual bool IsDrawing { get => _Currentpath != null; }

        public virtual void PathStart(SKPoint point)
        {
            _Currentpath = new SKPath();
            _Currentpath.MoveTo(point);
            _points.Clear();
        }

        public virtual void AddPointtoPath(SKPoint point)
        {
            _CheckIfPathIsNull();

            _points.Add(point);
            var smoothedPoint = GetSmoothedPoint();
            _Currentpath.LineTo(smoothedPoint.X, smoothedPoint.Y);
        }

        public virtual void PathEnd()
        {
            _Currentpath = null;
            //TODO: retornar info necesaria para aplicar el deshacer y rehacer
        }
		public virtual void Paint(SKCanvas canvas)
        {
            _CheckIfPathIsNull();
        }

        private void _CheckIfPathIsNull()
        {
            if (_Currentpath == null)
                throw new ArgumentNullException(nameof(_Currentpath));
        }

        public SKPoint GetSmoothedPoint()
        {

            switch (_points.Count)
            {
                case 1: case 2: case 3:
                    return _points.First();
            }

            // Get the last four points
            var n = _points.Count - 1;
            var p1 = _points[n - 3];
            var p2 = _points[n - 2];
            var p3 = _points[n - 1];
            var p4 = _points[n];

            // Calculate the smoothing factor based on a variable
            var smoothingFactor = Math.Min(1.0f, Math.Max(0.0f, Stabilization));

            // Calculate the smoothed point
            var x = smoothingFactor * (p1.X + 2 * p2.X + 2 * p3.X + p4.X) / 6f + (1 - smoothingFactor) * p3.X;
            var y = smoothingFactor * (p1.Y + 2 * p2.Y + 2 * p3.Y + p4.Y) / 6f + (1 - smoothingFactor) * p3.Y;

            return new SKPoint(x, y);
        }
    }

    public class PenBrush : Brush
    {
        private SKPaint _paint;

        public float Radius { get; set; }
        public SKColor Color { get; set; }

        private void _RegeneratePen()
        {
            _paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true,
                StrokeWidth = Radius,
                StrokeMiter = 10,
                Color = Color,
            };
        }

        public override void PathStart(SKPoint point)
        {
            base.PathStart(point);

            _RegeneratePen();
        }

        public override void Paint(SKCanvas canvas)
        {
            base.Paint(canvas);

            canvas.DrawPath(_Currentpath, _paint);
        }
    }

    public class TextureBrush : Brush
    {
        private SKPaint _paint;
        private SKBitmap _image;
        private SKBitmap _resized;

        public string Texture { get; private set; }
        public float Interval { get; set; } = 5f;
        public float Blur { get; set; }
        public SKColor Color { get; set; }
        public bool ColorEnabled { get; set; }
        public float Radius { get; set; }

        public TextureBrush(string texturePath)
        {
            if (string.IsNullOrEmpty(texturePath))
                throw new ArgumentNullException(nameof(texturePath));

            Texture = texturePath;
        }

        public override void PathStart(SKPoint point)
        {
            base.PathStart(point);
            _InitializePaint();
        }

        private void _InitializePaint()
        {
            if (_image == null)
            {
                var textureStream = FileSystem.OpenAppPackageFileAsync(Texture).Result;
                _image = SKBitmap.Decode(textureStream);
            }

            _paint = new SKPaint
            {
                IsAntialias = true,
                Color = Color,
                ColorFilter = ColorEnabled? SKColorFilter.CreateBlendMode(Color, SKBlendMode.SrcIn) : null
            };

            _resized = _image.Resize(new SKImageInfo(((int)Radius * 2), ((int)Radius * 2)), SKFilterQuality.High);

            var blurFilter = SKImageFilter.CreateBlur(Blur, Blur);
            _paint.ImageFilter = blurFilter;
        }

        public override void Paint(SKCanvas canvas)
        {
            base.Paint(canvas);

            SKPathMeasure pathMeasure = new SKPathMeasure(_Currentpath, false);
            var pathLength = pathMeasure.Length;

            for (float distance = 0; distance < pathLength; distance += Interval)
            {
                SKMatrix matrix = new SKMatrix();
                matrix = pathMeasure.GetMatrix(distance, SKPathMeasureMatrixFlags.GetPosition);
                
                canvas.DrawBitmap(_resized, pathMeasure.GetPosition(distance), _paint);
            }
        }
    }

    public class PatternBrush : Brush
    {
        private SKPaint _paint;
        private SKBitmap _bm;

        public string Pattern { get; private set; }
        public SKColor Color { get; set; }
        public bool ColorEnabled { get; set; }
        public float Radius { get; set; }
        public float PattenrSize { get; set; }

        public PatternBrush(string patternPath)
        {
            if (string.IsNullOrEmpty(patternPath))
                throw new ArgumentNullException(nameof(patternPath));

            Pattern = patternPath;
        }

        public override void PathStart(SKPoint point)
        {
            base.PathStart(point);
            _InitializePaint();
        }

        private void _InitializePaint()
        {
            if (_bm == null)
            {
                var textureStream = FileSystem.OpenAppPackageFileAsync(Pattern).Result;
                var textureData = SKData.Create(textureStream);
                var textureImage = SKImage.FromEncodedData(textureData);
                _bm = SKBitmap.FromImage(textureImage);
            }
            var resized = _bm.Resize(new SKImageInfo(((int)PattenrSize * 2), ((int)PattenrSize * 2)), SKFilterQuality.High);
            var shader = SKShader.CreateBitmap(resized, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
            _paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true,
                StrokeWidth = Radius * 2,
                Shader = shader,
                StrokeJoin = SKStrokeJoin.Round,
                BlendMode = SKBlendMode.SrcOver,
                ColorFilter = ColorEnabled? SKColorFilter.CreateBlendMode(SKColors.Blue, SKBlendMode.SrcIn) : null
            };
        }

        public override void Paint(SKCanvas canvas)
        {
            base.Paint(canvas);
            canvas.DrawPath(_Currentpath, _paint);
        }
    }

    /*public class AirBrush : Brush
    {
        private SKPaint _paint;
        private SKBitmap _bm;

        public float Rotation { get; set; }
        public float Scatter { get; set; }
        public int ScatterCount { get; set; }
        public string Texture { get; private set; }
        public SKColor Color { get; set; }
        public bool ColorEnabled { get; set; }
        public float Radius { get; set; }
        public float TextureSize { get; set; }

        public AirBrush(string texturePath)
        {
            if (string.IsNullOrEmpty(texturePath))
                throw new ArgumentNullException(nameof(texturePath));

            Texture = texturePath;
        }

        public override void PathStart(SKPoint point)
        {
            base.PathStart(point);

            _Initializepaint();
        }

        private void _Initializepaint()
        {
            if (_bm == null)
            {
                var textureStream = FileSystem.OpenAppPackageFileAsync(Texture).Result;
                var textureData = SKData.Create(textureStream);
                var textureImage = SKImage.FromEncodedData(textureData);
                var bm = SKBitmap.FromImage(textureImage);
                _bm = bm.Resize(new SKImageInfo(((int)TextureSize * 2), ((int)TextureSize * 2)), SKFilterQuality.High);
            }

            _paint = new SKPaint
            {
                //Style = SKPaintStyle.Stroke,
                //StrokeCap = SKStrokeCap.Round,
                //IsAntialias = true,
                //StrokeWidth = Radius * 2,
                //StrokeJoin = SKStrokeJoin.Round,
                //BlendMode = SKBlendMode.SrcOver,
                //ColorFilter = ColorEnabled ? SKColorFilter.CreateBlendMode(SKColors.Blue, SKBlendMode.SrcIn) : null
                Color = Color
            };
        }

        public override void Paint(SKCanvas canvas)
        {
            base.Paint(canvas);
            for (int i = 0; i < ScatterCount; i++)
            {
                float scatterX = (float)(2 * Scatter * (new Random().NextDouble() - 0.5));
                float scatterY = (float)(2 * Scatter * (new Random().NextDouble() - 0.5));

                SKMatrix matrix = SKMatrix.CreateTranslation(scatterX, scatterY);

                if (Rotation != 0)
                {
                    float rotationAngle = (float)(2 * Rotation * (new Random().NextDouble() - 0.5));
                    //matrix = SKMatrix.CreateRotationDegrees(rotationAngle, _Currentpath.LastPoint.X + scatterX, _Currentpath.LastPoint.Y + scatterY) * matrix;
                    matrix = SKMatrix.Concat(SKMatrix.CreateRotationDegrees(rotationAngle, _Currentpath.LastPoint.X + scatterX, _Currentpath.LastPoint.Y + scatterY), matrix);
                }

                using (var shader = SKShader.CreateBitmap(_bm, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, matrix))
                {
                    _paint.Shader = shader;
                    canvas.DrawPath(_Currentpath, _paint);
                }
            }
        }
    }*/

    public class AirBrush : Brush
    {
        private SKPaint _paint;
        private SKBitmap _image;
        private SKPoint _currentPoint;

        public string Texture { get; private set; }
        public float Interval { get; set; } = 5f;
        public float Blur { get; set; }
        public SKColor Color { get; set; }
        public bool ColorEnabled { get; set; }
        public float Radius { get; set; }
        public int IterationsCount { get; set; }
        public float RandomFactor { get; set; }
        public float SizeVariation { get; set; }
        public float AlphaVariation { get; set; }
        public float RotationVariation { get; set; }

        public AirBrush(string texturePath)
        {
            if (string.IsNullOrEmpty(texturePath))
                throw new ArgumentNullException(nameof(texturePath));

            Texture = texturePath;
        }

        public override void PathStart(SKPoint point)
        {
            base.PathStart(point);

            _currentPoint = point;
            _InitializePaint();
        }

        public override void AddPointtoPath(SKPoint point)
        {
            base.AddPointtoPath(point);
            var sp = GetSmoothedPoint();
            _currentPoint = sp;
        }

        private void _InitializePaint()
        {
            if (_image == null)
            {
                var textureStream = FileSystem.OpenAppPackageFileAsync(Texture).Result;
                var bm = SKBitmap.Decode(textureStream);

                _image = bm.Resize(new SKImageInfo(((int)Radius * 2), ((int)Radius * 2)), SKFilterQuality.High);
            }

            _paint = new SKPaint
            {
                IsAntialias = true,
                Color = Color,
                ColorFilter = ColorEnabled ? SKColorFilter.CreateBlendMode(Color, SKBlendMode.SrcIn) : null
            };

            var blurFilter = SKImageFilter.CreateBlur(Blur, Blur);
            _paint.ImageFilter = blurFilter;
        }

        public override void Paint(SKCanvas canvas)
        {
            base.Paint(canvas);

            for (var i = 0; i < IterationsCount; i++)
            {
                var x = (float)(2 * RandomFactor * (new Random().NextDouble() - 0.5));
                var y = (float)(2 * RandomFactor * (new Random().NextDouble() - 0.5));
                var newPoit = new SKPoint(_currentPoint.X + x, _currentPoint.Y + y);

                if (AlphaVariation != 0)
                {
                    var alpha = (float)(AlphaVariation * (new Random().NextDouble()));
                    _paint.ColorFilter = SKColorFilter.CreateBlendMode(Color.WithAlpha((byte)alpha), SKBlendMode.SrcIn);
                }

                var toUse = _image;

                if (SizeVariation != 0f)
                {
                    var v = (int)(SizeVariation * new Random().NextDouble());
                    //var h = (int)(SizeVariation * new Random().NextDouble());
                    //var matrix = SKMatrix.CreateScale(v, v);

                    //_paint.Shader = shader;

                    var resized = _image.Resize(new SKImageInfo(v, v), SKFilterQuality.High);
                    if (resized != null)
                        toUse = resized;

                    //canvas.DrawRect(SKRect.Create(newPoit.X, newPoit.Y, v, v), _paint);
                    /*var matrix = SKMatrix.CreateScale(v, v);
                    canvas.Concat(ref matrix);

                    canvas.DrawBitmap(_image, newPoit.X - _image.Width / 2, newPoit.Y - _image.Height / 2, _paint);*/
                }

                var degress = (float)(RotationVariation * (new Random().NextDouble())) * GetRandomSign();
                var centerX = toUse.Width / 2f;
                var centerY = toUse.Height / 2f;
                //var matrix = SKMatrix.CreateRotation(degress);
                var matrix = SKMatrix.CreateIdentity();
                matrix = SKMatrix.CreateTranslation(centerX, centerY);
                matrix = SKMatrix.Concat(matrix, SKMatrix.CreateRotationDegrees(degress));
                matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation(-centerX, -centerY));
                var rotatedBitmap = toUse.Copy();
                var surface = SKSurface.Create(new SKImageInfo(toUse.Width * 2, toUse.Height * 2));
                using var rotatedCanvas = surface.Canvas;
                rotatedCanvas.Concat(ref matrix);

                rotatedCanvas.DrawBitmap(toUse, centerX, centerY, _paint);
                //toUse = rotatedCanvas.

                //canvas.DrawBitmap(rotatedBitmap, newPoit, _paint);

                canvas.DrawSurface(surface, newPoit);
            }
        }
        private static int GetRandomSign()
        {
            Random random = new Random();
            int randomNumber = random.Next(0, 2);
            return (randomNumber == 0) ? -1 : 1;
        }
    }

    public class ShapeBrush : Brush
    {
        private SKCanvas _lastCanvas;
        private SKPaint _paint;
        public SKColor Color { get; set; }
        private void _RegeneratePen()
        {
            _paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true,
                StrokeWidth = 5,
                StrokeMiter = 10,
                Color = Color,
            };
        }

        public override void PathEnd()
        {
            if (_lastCanvas != null)
            {
                _paint.Style = SKPaintStyle.StrokeAndFill;
                _Currentpath.Close();
                //_lastCanvas.DrawPath(_Currentpath, _paint);
            }
            //base.PathEnd();
        }

        public override void PathStart(SKPoint point)
        {
            base.PathStart(point);

            _RegeneratePen();
        }

        public override void Paint(SKCanvas canvas)
        {
            base.Paint(canvas);

            canvas.DrawPath(_Currentpath, _paint);
            _lastCanvas = canvas;
        }
    }
}

