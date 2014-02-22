using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Local;

namespace DragDropDemo.ViewModels
{
    public class DragDropViewModel 
    {
        public ICommand DropCommand { get; private set; }
        public ICommand MapLoadedCommand { get; private set; }
        public ICommand DragMouseLeftButtonDownCommand { get; private set; }
        public ICommand DragGiveFeedbackCommand { get; private set; }

        private Map _map;

        public DragDropViewModel()
        {
            DropCommand = new RelayCommand(Dropped);
            MapLoadedCommand = new RelayCommand(MapLoaded);
            DragGiveFeedbackCommand = new RelayCommand(DragGiveFeedback);
            DragMouseLeftButtonDownCommand = new RelayCommand(DragMouseLeftButtonDown);
        }

        private void DragMouseLeftButtonDown(object e)
        {
            var args = e as MouseButtonEventArgs;
            if (args == null)
                return;

            DataObject data = new DataObject(DataFormats.Text, "Drag Data");

            DragDrop.DoDragDrop((DependencyObject)args.Source, data, DragDropEffects.Copy);
        }

        private void DragGiveFeedback(object e)
        {
            var args = e as GiveFeedbackEventArgs;
            if (args == null)
                return;
            
            if (args.Effects == DragDropEffects.Copy)
            {
                args.UseDefaultCursors = false;
                Mouse.SetCursor(Cursors.Cross);
            }
            else
                args.UseDefaultCursors = true;

            args.Handled = true;
        }

        private void Dropped(object parameter)
        {
            var args = parameter as DragEventArgs;
            if (args == null)
                return;

            var mousePosition = args.GetPosition(_map);
            var mapPoint = _map.ScreenToMap(mousePosition);

            if (_map.WrapAroundIsActive)
                mapPoint = Geometry.NormalizeCentralMeridian(mapPoint) as MapPoint;

            var newpoint = new Graphic();
            newpoint.Geometry = mapPoint;

            var layer = _map.Layers["FlagsLayer"] as GraphicsLayer;
            layer.Graphics.Add(newpoint);
        }

        private void Label_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effects == DragDropEffects.Copy)
            {
                e.UseDefaultCursors = false;
                Mouse.SetCursor(Cursors.Pen);
            }
            else
                e.UseDefaultCursors = true;

            e.Handled = true;
        }

        /// <summary>
        /// Need a way of capturing a reference to the map. 
        /// </summary>
        private void MapLoaded(object parameter)
        {
            // See if the event sent the map in the event args
            var p1 = parameter as System.Windows.RoutedEventArgs;
            if (p1 == null)
            {
                throw new NullReferenceException("Cannot capture map reference.");
                return;
            }

            _map = p1.Source as Map;
        }
    }
}
