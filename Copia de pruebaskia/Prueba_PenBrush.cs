using System;
using pruebaskia.Painter;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace pruebaskia
{
	public class Prueba_PenBrush : SKCanvasView
	{
		private PenBrush _brush;
		private SKCanvas _canvas;
		public Prueba_PenBrush()
		{
			EnableTouchEvents = true;
			_brush = new PenBrush
			{
				Color = SKColors.Blue,
				Radius = 10f,
				Stabilization = 0.5f
			};
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
			_canvas = e.Surface.Canvas;

			if (_brush.IsDrawing)
				_brush.Paint(_canvas);
        }
    }
}

