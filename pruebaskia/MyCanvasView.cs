using System;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace pruebaskia
{
    public class MyCanvasView : SKCanvasView
    {
        private SKCanvasView _view = new SKCanvasView();
        private SKCanvas _canvas;
        private SKPaint _paint;
        private bool _isDrawing = false;
        private SKPoint _lastPoint;
        private Action _lastAction = null;
        private List<SKPoint> _points = new List<SKPoint>();
        private SKPath _currentPath = null;
        private SKBitmap _image = null;
        private float _smoothingFactor = 0.5f;
        public MyCanvasView()
        {
            this.EnableTouchEvents = true;
            _loadTexture().Wait();
        }
        protected override void OnTouch(SKTouchEventArgs e)
        {
            //base.OnTouch(e);

            this.InvalidateSurface();

            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    // Comenzar a dibujar
                    _isDrawing = true;
                    //_lastPoint = e.Location;

                    _points.Clear();
                    //_points.Add(e.Location);

                    _currentPath = new SKPath();
                    //_currentPath.LineTo(e.Location);
                    _currentPath.MoveTo(e.Location);
                    break;
                case SKTouchAction.Moved:
                    if (_isDrawing)
                    {
                        _points.Add(e.Location);
                        // Calcular la distancia del trazo anterior
                        //var distance = _lastPoint.DistanceTo(e.Location);
                        // Obtener el ancho del trazo
                        //var strokeWidth = _getStrokeWidth(distance);
                        // Establecer el ancho del trazo
                        //_paint.StrokeWidth = strokeWidth;
                        // Dibujar la línea
                        try
                        {

                            var smoothedPoint = GetSmoothedPoint();
                            _lastPoint = e.Location;
                            _currentPath.LineTo(smoothedPoint.X, smoothedPoint.Y);
                            _lastAction = () =>
                            {
                                if (_currentPath != null)
                                {
                                    SKPathMeasure pathMeasure = new SKPathMeasure(_currentPath, false);
                                    float pathLength = pathMeasure.Length;

                                    // Configurar el pincel con la textura deseada
                                    SKPaint paint = new SKPaint();
                                    paint.Shader = SKShader.CreateBitmap(_image, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
                                    paint.ColorFilter = SKColorFilter.CreateBlendMode(SKColors.Blue, SKBlendMode.SrcOver);
                                    var skB = new SKBitmap();
                                    _image.ExtractAlpha(skB);
                                    paint.BlendMode = SKBlendMode.Modulate;
                                    // Dibujar la textura a lo largo de la curva
                                    float interval = 5; // ajustar según se desee la separación entre las texturas
                                    for (float distance = 0; distance < pathLength; distance += interval)
                                    {
                                        SKMatrix matrix = new SKMatrix();
                                        matrix = pathMeasure.GetMatrix(distance, SKPathMeasureMatrixFlags.GetPosition);
                                        var resized = skB.Resize(new SKImageInfo(32, 32), SKFilterQuality.High);
                                        if (_currentPath != null)
                                            _canvas.DrawBitmap(resized, pathMeasure.GetPosition(distance), paint);
                                    }
                                }
                                //if (_currentPath != null)
                                //_canvas.DrawPath(_currentPath, _paint);
                            };
                        }
                        catch (Exception ex)
                        {

                        }
                        //_lastPoint = e.Location;
                    }
                    break;
                case SKTouchAction.Released:
                    // Terminar de dibujar
                    _isDrawing = false;
                    // Add the current path to the list of paths
                    //_currentLayer.Paths.Add(_currentPath);
                    _currentPath = null;
                    break;
            }

            // Indicar que se procesó el evento de toque
            e.Handled = true;
            this.InvalidateMeasure();
        }
        private async Task _loadTexture()
        {
            // Cargar la textura
            var textureStream = await FileSystem.OpenAppPackageFileAsync("circle.png");
            var textureData = SKData.Create(textureStream);
            var codec = SKCodec.Create(textureData);
            var textureImage = SKImage.FromEncodedData(textureData);
            //using var ms = new MemoryStream();
            textureStream.Position = 0;
            //textureStream.CopyTo(ms);
            _image = SKBitmap.Decode(textureStream);
            var bm = SKBitmap.FromImage(textureImage);
            var shader = SKShader.CreateBitmap(bm, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
            // Configurar la brocha de dibujo para usar la textura
            _paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true,
                StrokeWidth = 10,
                StrokeMiter = 10,
                Shader = shader,
                StrokeJoin = SKStrokeJoin.Round,
                BlendMode = SKBlendMode.SrcOver,
                ColorFilter = SKColorFilter.CreateBlendMode(SKColors.Blue, SKBlendMode.Color)
                //Color = SKColor.FromHsl(0, 0, 0)
            };
        }
        private float _getStrokeWidth(float distance)
        {
            // Calcular el ancho del trazo basado en la distancia del trazo anterior
            if (distance < 0.1f)
            {
                return 12f;
            }
            else if (distance < 0.5f)
            {
                return 12f - (distance * 20f);
            }
            else if (distance < 1f)
            {
                return 2f;
            }
            else
            {
                return 1f;
            }
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            //this.InvalidateSurface();
            _canvas = e.Surface.Canvas;
            _canvas.Clear(SKColors.White);
            _lastAction?.Invoke();
        }
        private SKPoint GetSmoothedPoint()
        {
            // Get the last four points
            var n = _points.Count - 1;
            var p1 = _points[n - 3];
            var p2 = _points[n - 2];
            var p3 = _points[n - 1];
            var p4 = _points[n];

            // Calculate the smoothing factor based on a variable
            var smoothingFactor = Math.Min(1.0f, Math.Max(0.0f, _smoothingFactor));

            // Calculate the smoothed point
            var x = smoothingFactor * (p1.X + 2 * p2.X + 2 * p3.X + p4.X) / 6f + (1 - smoothingFactor) * p3.X;
            var y = smoothingFactor * (p1.Y + 2 * p2.Y + 2 * p3.Y + p4.Y) / 6f + (1 - smoothingFactor) * p3.Y;

            return new SKPoint(x, y);
        }
    }
}
public static class SKPointExtensions
{
    public static float DistanceTo(this SKPoint point1, SKPoint point2)
    {
        float dx = point2.X - point1.X;
        float dy = point2.Y - point1.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }
}

