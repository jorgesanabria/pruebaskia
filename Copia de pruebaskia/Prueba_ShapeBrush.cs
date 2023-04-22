using System;
using pruebaskia.Painter;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace pruebaskia
{
	public class Prueba_ShapeBrush : SKCanvasView
    {
        private ShapeBrush _brush;
        public Prueba_ShapeBrush()
		{
			_brush = new ShapeBrush
			{
				Color = SKColors.Blue,
				Stabilization = 5f
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

