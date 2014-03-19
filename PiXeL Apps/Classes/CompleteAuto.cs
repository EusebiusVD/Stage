using PiXeL_Apps.SQLite.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.Classes
{
    public class CompleteAuto
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public int Merk_Id { get; set; } //Linken
        public string Merk_Naam { get; set; }
        public string Merk_Type { get; set; }
        public string Snelheid_Type { get; set; }
        public string Kilometerstand { get; set; }
        public string Oliepeil { get; set; }
        public int Motor_Id { get; set; } //Linken
        public string Motor_Code { get; set; }
        public decimal Motor_Capaciteit { get; set; }
        public int Motor_Brandstof_Id { get; set; }
        public string Motor_Brandstof_Naam { get; set; }
        public int Transmissie_Id { get; set; } //Linken
        public string Transmissie_Naam { get; set; }
        public int Transmissie_Versnellingen { get; set; }
        public int Eenheid_Id { get; set; }
        public string Eenheid_Naam { get; set; }
        public bool Active { get; set; }

        public CompleteAuto(int id, string number)
        {
            Id = id;
            Number = number;
        }
        public CompleteAuto(int id, string number, string oliepeil)
        {
            Id = id;
            Number = number;
            Oliepeil = oliepeil;
        }

        public CompleteAuto(int id, string number, int brandId, int engineId, int transmissionId, int eenheidId, bool active) 
        {
            Id = id;
            Number = number;
            Merk_Id = brandId;
            Motor_Id = engineId;
            Transmissie_Id = transmissionId;
            Active = active;
        }

        public CompleteAuto(int id, string number,
            int merkId, string merkNaam, string merkType,
            int engineId, string motorCode, decimal motorCapaciteit, int motorBrandstofId,
            int transmissieId, string transmissieNaam, int transmissieVersnellingen,
            int eenheidId, bool active) 
        {
            Id = id;
            Number = number;
            Merk_Id = merkId;
            Merk_Naam = merkNaam;
            Merk_Type = merkType;
            Motor_Id = engineId;
            Motor_Code = motorCode;
            Motor_Capaciteit = motorCapaciteit;
            Motor_Brandstof_Id = motorBrandstofId;
            Transmissie_Id = transmissieId;
            Transmissie_Naam = transmissieNaam;
            Transmissie_Versnellingen = transmissieVersnellingen;
            Eenheid_Id = eenheidId;
            if (eenheidId == 1)
                Eenheid_Naam = "Mijl";
            else
                Eenheid_Naam = "Kilometer";
            Active = active;
        }

        public void SetBrand(Brand brand)
        {
            Merk_Id = brand.Id;
            Merk_Naam = brand.BrandName;
            Merk_Type = brand.Type;
        }

        public Brand GetBrand()
        {
            return new Brand(Merk_Id, Merk_Naam, Merk_Type);
        }

        public void SetEngine(Engine engine)
        {
            Motor_Id = engine.Id;
            Motor_Code = engine.Engine_Code;
            Motor_Capaciteit = Convert.ToDecimal(engine.Capacity);
            Motor_Brandstof_Id = engine.Id;
        }

        public Engine GetEngine()
        {
            return new Engine(Motor_Id, Motor_Code, Motor_Capaciteit, Motor_Brandstof_Id);
        }

        public void SetFuel(Fuel fuel)
        {
            Motor_Brandstof_Id = fuel.Id;
            Motor_Brandstof_Naam = fuel.FuelName;
        }

        public Fuel GetFuel()
        {
            return new Fuel(Motor_Brandstof_Id, Motor_Brandstof_Naam);
        }

        public void SetTransmissie(Transmission transmissie)
        {
            Transmissie_Id = transmissie.Id;
            Transmissie_Naam = transmissie.Transmission_Name;
            Transmissie_Versnellingen = Convert.ToInt32(transmissie.Gears);
        }

        public Transmission GetBrandstof()
        {
            return new Transmission(Transmissie_Id, Transmissie_Naam, Transmissie_Versnellingen);
        }

        public void SetEenheid(int id)
        {
            Eenheid_Id = id;
            if (id == 1)
                Eenheid_Naam = "Mijl";
            else
                Eenheid_Naam = "Kilometer";
        }

        public string GetEenheid()
        {
            return Eenheid_Naam;
        }

        public string getOilLevel()
        {
            return Oliepeil;
        }
    }
}