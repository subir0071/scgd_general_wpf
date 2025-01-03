﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ColorVision.ImageEditor.Draw
{

    public class DrawCanvas : Image
    {
        public DrawCanvas()
        {
            this.Focusable = true;
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, (s, e) => Undo()));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, (s, e) => Redo()));

        }

        private List<Visual> visuals = new();

        private Stack<Action> undoStack = new Stack<Action>();
        private Stack<Action> redoStack = new Stack<Action>();


        protected override Visual GetVisualChild(int index) => visuals[index];

        protected override int VisualChildrenCount { get => visuals.Count; }

        public bool ContainsVisual(Visual visual) => visuals.Contains(visual);

        public event EventHandler? ImageInitialized;

        public void ImageInitialize()
        {
            ImageInitialized?.Invoke(this, new EventArgs());
        }


        public event EventHandler? VisualsChanged;

        public event EventHandler? VisualsAdd;
        public event EventHandler? VisualsRemove;

        public void Clear()
        {
            foreach (var item in visuals)
            {
                RemoveVisualChild(item);
                RemoveLogicalChild(item);
            }
            visuals.Clear();

        }
        public void OnlyAddVisual(Visual visual)
        {
            visuals.Add(visual);

            AddVisualChild(visual);
            AddLogicalChild(visual);
        }


        public void AddVisual(Visual visual, bool recordAction = true)
        {
            try
            {
                visuals.Add(visual);

                AddVisualChild(visual);
                AddLogicalChild(visual);
                VisualsAdd?.Invoke(visual, EventArgs.Empty);
                VisualsChanged?.Invoke(visual, EventArgs.Empty);

                if (recordAction)
                {
                    undoStack.Push(() => RemoveVisual(visual, false));
                    redoStack.Clear();
                }
            }
            catch
            {

            }

        }

        public void RemoveVisual(Visual? visual, bool recordAction = true)
        {
            if (visual == null) return;
            visuals.Remove(visual);

            RemoveVisualChild(visual);
            RemoveLogicalChild(visual);
            VisualsRemove?.Invoke(visual, EventArgs.Empty);
            VisualsChanged?.Invoke(visual, EventArgs.Empty);

            if (recordAction)
            {
                undoStack.Push(() => AddVisual(visual,false));
                redoStack.Clear();
            }
        }
        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                var undoAction = undoStack.Pop();
                undoAction();
            }
        }
        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                var redoAction = redoStack.Pop();
                redoAction();
            }
        }
        public void TopVisual(Visual visual)
        {
            RemoveVisualChild(visual);
            RemoveLogicalChild(visual);

            AddVisualChild(visual);
            AddLogicalChild(visual);
            VisualsChanged?.Invoke(visual, EventArgs.Empty);

        }


        public DrawingVisual? GetVisual(Point point)
        {
            HitTestResult hitResult = VisualTreeHelper.HitTest(this, point);

            if (hitResult == null)
                return null;
            return hitResult.VisualHit as DrawingVisual;
        }



        private List<DrawingVisual> hits = new();
        public List<DrawingVisual> GetVisuals(Geometry region)
        {
            hits.Clear();
            GeometryHitTestParameters parameters = new(region);
            HitTestResultCallback callback = new(HitTestCallback);
            VisualTreeHelper.HitTest(this, null, callback, parameters);
            return hits;
        }

        private HitTestResultBehavior HitTestCallback(HitTestResult result)
        {
            GeometryHitTestResult geometryResult = (GeometryHitTestResult)result;
            DrawingVisual visual = result.VisualHit as DrawingVisual;
            if (visual != null &&
                geometryResult.IntersectionDetail == IntersectionDetail.FullyInside)
            {
                hits.Add(visual);
            }
            return HitTestResultBehavior.Continue;
        }



    }

}
