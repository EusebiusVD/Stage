using PiXeL_Apps.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PiXeL_Apps.Common
{
    public class DataTemplateKiezer : DataTemplateSelector
    {
        public DataTemplate VoltooideInspectieTemplate { get; set; }
        public DataTemplate OnvoltooideInspectieTemplate { get; set; }
        public DataTemplate StandaardInspectieTemplate { get; set; }
        public DataTemplate BelangrijkInspectieTemplate { get; set; }
        public DataTemplate DringendInspectieTemplate { get; set; }

        /// <summary>
        /// Afhankelijk van het aantal gereden kilometers en de kilometers tot de volgende inspectie wordt een gepast DataTemplate meegegeven.
        /// Het inspectie gridview gebruikt dit om te bepalen wat de layout van elke inspectie moet zijn. Dit geeft aan hoe belangrijk of dichtbij een inspectie is.
        /// </summary>
        /// <param name="item">Inspectie object</param>
        /// <param name="container"></param>
        /// <returns>DataTemplate</returns>
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            Inspectie inspectie = item as Inspectie;
            int kilometersGereden = GpsSupport.gpsSupport.GetKilometersGereden();
            if (kilometersGereden < inspectie.Kilometerstand)
            {
                int tolerantieAankomend = GpsSupport.gpsSupport.GetTolerantieAankomend() + 1;
                int tolerantieDringend = GpsSupport.gpsSupport.GetTolerantieDringend() + 1;
                if (kilometersGereden > inspectie.Kilometerstand - tolerantieDringend)
                    return this.DringendInspectieTemplate;
                else if (kilometersGereden > inspectie.Kilometerstand - tolerantieAankomend)
                    return this.BelangrijkInspectieTemplate;
                else
                    return this.StandaardInspectieTemplate;
            }
            else
                if (inspectie.Status)
                    return this.VoltooideInspectieTemplate;
                else
                    return this.OnvoltooideInspectieTemplate;
        }
    }
}
