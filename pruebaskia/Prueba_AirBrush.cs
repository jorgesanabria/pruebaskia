using System;
using pruebaskia.Painter;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace pruebaskia
{
	public class Prueba_AirBrush : SKCanvasView
	{
		private AirBrush _brush;
		public Prueba_AirBrush()
		{
			_brush = new AirBrush("butterfly.png")
			{
				Color = SKColors.Blue,
				ColorEnabled = true,
				//Rotation = 5f,
				//Scatter = 2f,
				//ScatterCount = 2,
                //TextureSize = 5,
                Radius = 100,
                IterationsCount = 1,
                RandomFactor = 100,
                //Interval = 200,
                //Blur = 5,
                SizeVariation = 100,
                AlphaVariation = 128,
                RotationVariation = 50
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

