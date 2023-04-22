using System;
using pruebaskia.Painter;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace pruebaskia
{
	public class Prueba_PatenrBrush : SKCanvasView
	{
		private PatternBrush _brush;
		public Prueba_PatenrBrush()
		{
			_brush = new PatternBrush("pattern.png")
			{
				Color = SKColors.Blue,
				Radius = 50,
				ColorEnabled = true,
                PattenrSize = 100
			};
            EnableTouchEvents = true;
        }

        protected override void OnTouch(SKTouchEventArgs e)
        {
            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    _brush.PathStart(e.Location);
                    break;
                case SKTouchAction.Moved:
                    _brush.AddPointtoPath(e.Location);
                    break;
                case SKTouchAction.Released:
                    _brush.PathEnd();
                    break;
            }
            e.Handled = true;
            InvalidateSurface();
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);
            //_canvas = e.Surface.Canvas;

            if (_brush.IsDrawing)
                _brush.Paint(e.Surface.Canvas);
        }
    }
}

