using System;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui;

namespace pruebaskia
{
	public class PruebaCanvasView : SKCanvasView
    {
        private SKCanvas _canvas;
        private Action _lastAction = null;
        public PruebaCanvasView()
		{
            EnableTouchEvents = true;
		}


        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            _canvas = e.Surface.Canvas;
            _canvas.Clear(SKColors.White);
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true,
                //Shader = shader,
                StrokeJoin = SKStrokeJoin.Round,
                BlendMode = SKBlendMode.SrcOver,
                Color = SKColor.FromHsl(0, 0, 0)
            };
            _lastAction?.Invoke();
        }
        
        protected override void OnTouch(SKTouchEventArgs e)
        {
            base.OnTouch(e);

            this.InvalidateSurface();
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true,
                //Shader = shader,
                StrokeJoin = SKStrokeJoin.Round,
                BlendMode = SKBlendMode.SrcOver,
                //StrokeWidth = 10,
                Color = SKColor.FromHsl(0, 0, 0)
            };
            //this.InvalidateSurface();
            e.Handled = true;
            _lastAction = () => _canvas.DrawLine(new SKPoint { X = 0, Y = 0 }, new SKPoint { X = 100, Y = 100 }, paint);
        }
    }
}

