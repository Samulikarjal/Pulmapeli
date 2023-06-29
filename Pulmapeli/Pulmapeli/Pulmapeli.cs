using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Net.Mime;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using Vector = Jypeli.Vector;

namespace Pulmapeli;

public class Pulmapeli : PhysicsGame
{
    private const double NOPEUS = 200;
    private const double hyppyNopeus = 1000;
    private const int RUUDUN_KOKO = 40;

    private PlatformCharacter pelaaja1;
    private int Avainkeratty = 0;
    private int kenttaNro = 1;
   
    
    private bool Nappia1Painettu = false;
    private bool Nappia2Painettu = false;
    private bool Nappia3Painettu = false;
    private bool Nappia4Painettu = false;
    private Vector reuna;
    private Image pelaajanKuva = LoadImage("norsu.png");
    private Image avainKuva = LoadImage("avain");
   
    private Image TaustaKuva = LoadImage("taustakuva");
    private Image TasonKuva = LoadImage("Taso");
    private Image VesiKuva = LoadImage("vesi");
    public  Image LiikkuvanTasonKuva = LoadImage("LiikkuvaTaso");
    private Image LiikkuvanTasonKuva2 = LoadImage("LiikkuvaTaso");
    private Image PomppuTasonKuva = LoadImage("pompputaso");
    private Image OvenKuva = LoadImage("ovi");
    private SoundEffect MaaliAani = LoadSoundEffect("maali.wav");
    private Image Seina = LoadImage("seina");
    private Image Nappi1 = LoadImage("nappi1");
    private Image Nappi2 = LoadImage("nappi2");
    private Image Nappi3 = LoadImage("nappi3");
    private Image Nappi4 = LoadImage("nappi4");
    private Image Hissi1 = LoadImage("hissi1");
    private Image Hissi2 = LoadImage("hissi2");
    private Image Hissi3 = LoadImage("hissi3");
    private Image Hissi4 = LoadImage("hissi4");

    private List<Nappi> napit = new List<Nappi>();
    private List<Hissi> hissit = new List<Hissi>();
   

    
    
    
    
    public override void Begin()
    {
       
        Gravity = new Vector(0, -1000);
       
       
        LisaaNappaimet();

        Camera.Follow(pelaaja1);
        Camera.ZoomFactor = 0.5;
        Camera.StayInLevel = true;

        MasterVolume = 0.5;
        PaaValikko();
    }

    private void LuoKentta(string kenttanro)
    {
        TileMap kentta = TileMap.FromLevelAsset(kenttanro);
        kentta.SetTileMethod('#', LisaaTaso);
        kentta.SetTileMethod('*', LisaaAvain);
        kentta.SetTileMethod('P', LisaaPelaaja);
        kentta.SetTileMethod('1', delegate(Vector position, double width, double height)
        {
            LisaaNappi(position, width, height, Nappi1, "1");
        });
        kentta.SetTileMethod('2', delegate(Vector position, double width, double height)
        {
            LisaaNappi(position, width, height, Nappi2, "2");
        });
        kentta.SetTileMethod('3', delegate(Vector position, double width, double height)
        {
            LisaaNappi(position, width, height, Nappi3, "3");
        });
        kentta.SetTileMethod('4', delegate(Vector position, double width, double height)
        {
            LisaaNappi(position, width, height, Nappi4, "4");
        });
        kentta.SetTileMethod('5', delegate(Vector position, double width, double height)
        {
            LisaaHissi(position, width, height, Hissi1, "1");
        });
        kentta.SetTileMethod('6', delegate(Vector position, double width, double height)
        {
            LisaaHissi(position, width, height, Hissi2, "2");
        });
        kentta.SetTileMethod('7', delegate(Vector position, double width, double height)
        {
            LisaaHissi(position, width, height, Hissi3, "3");
        });
        kentta.SetTileMethod('8', delegate(Vector position, double width, double height)
        {
            LisaaHissi(position, width, height, Hissi4, "4");
        });
        kentta.SetTileMethod('T', LisaaVesi);
        kentta.SetTileMethod('p', LisaaPomppuTaso);
        kentta.SetTileMethod('O', LisaaOvi);
        kentta.SetTileMethod('M', LisaaMaali);
        kentta.SetTileMethod('S', LisaaSeina);
       
        
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO*2);
        AlustaNapit();
        Level.Background.Image = TaustaKuva;
        Level.Background.ScaleToLevelFull();
        Camera.Follow(pelaaja1);
        Camera.ZoomFactor = 0.5;
        //Camera.ZoomToAllObjects();
        //Camera.StayInLevel = true;

        MasterVolume = 0.5;
        LisaaNappaimet();

    }

    void AlustaNapit()
    {
        foreach (var nappi in napit)
        {
            foreach (var hissi in hissit)
            {
                if (hissi.Tag.ToString() != nappi.Tag.ToString())
                {
                    continue;
                }

                nappi.hissi = hissi;
                break;
            }
        }
    }
    void SeuraavaKentta()
    {
        ClearAll();
        if (kenttaNro > 4) Exit();
        LuoKentta($"kentta{kenttaNro}");
        

    }

    void Kentta1()
    {
        kenttaNro = 1;
        SeuraavaKentta();
    }

    void Kentta2()
    {
        kenttaNro = 2;
        SeuraavaKentta();
    }

    void Kentta3()
    {
        kenttaNro = 3;
        SeuraavaKentta();
    }

    void Kentta4()
    {
        kenttaNro = 4;
        SeuraavaKentta();
    }
    void PaaValikko()
    {
        MultiSelectWindow paaValikko = new MultiSelectWindow("Main menu", "Level 1", "Level 2", "Level 3", "Level 4", "Exit"); 
        Add(paaValikko);
        paaValikko.AddItemHandler(0, Kentta1);
        paaValikko.AddItemHandler(1, Kentta2);
        paaValikko.AddItemHandler(2, Kentta3);
        paaValikko.AddItemHandler(3, Kentta4);
        paaValikko.AddItemHandler(4, Exit);
    }

    void LapaisyValikko()
    {
        MultiSelectWindow lapaisyvalikko = new MultiSelectWindow("Level completed!","Next level", "Main Menu", "Exit");
        Add(lapaisyvalikko);
        lapaisyvalikko.AddItemHandler(0, SeuraavaKentta);
        lapaisyvalikko.AddItemHandler(1, PaaValikko);
        lapaisyvalikko.AddItemHandler(2, Exit);
    }
    

    void LisaaVesi(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject vesi = PhysicsObject.CreateStaticObject(leveys, korkeus);
        vesi.Position = paikka;
        vesi.Image = VesiKuva;
        vesi.Tag = "vesi";
        Add(vesi);
    }
    private void LisaaTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus/2);
        taso.Position = paikka;
        taso.Image = TasonKuva;
        Add(taso);
    }

    void LisaaOvi(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject ovi = PhysicsObject.CreateStaticObject(leveys*4, korkeus*4);
        ovi.Position = paikka;
        ovi.Image = OvenKuva;
        ovi.IgnoresGravity = true;
        ovi.IgnoresCollisionResponse = true;
        ovi.Y = paikka.Y + 80;
        ovi.Tag = "pomppu";
        Add(ovi, 1);
    }
    void LisaaSeina(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject seina = PhysicsObject.CreateStaticObject(leveys, korkeus);
        seina.Position = paikka;
        seina.Image = Seina;
        Add(seina, 1);
    }

   
    void LisaaPomppuTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject PomppuTaso = PhysicsObject.CreateStaticObject(leveys*4, korkeus*2);
        PomppuTaso.Position = paikka;
        PomppuTaso.Image = PomppuTasonKuva;
     
        PomppuTaso.Tag = "pomppu";
        Add(PomppuTaso);
    }
  

  

    private void LisaaAvain(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject avain = PhysicsObject.CreateStaticObject(leveys*5, korkeus*3);
        avain.IgnoresCollisionResponse = true;
        avain.Position = paikka;
        avain.Y = paikka.Y + 100;
        avain.Image = avainKuva;
        avain.Tag = "avain";
        Add(avain);
    }

    private void LisaaNappi(Vector paikka, double leveys, double korkeus, Image kuva, string tagi)
    {
        Nappi nappi = new Nappi(leveys*5,korkeus*3);
        
        nappi.Position = paikka;
        nappi.IgnoresCollisionResponse = true;
        nappi.IgnoresGravity = true;
        nappi.Image = kuva;
        nappi.Tag = tagi;
        Add(nappi);
        napit.Add(nappi);
    }
    
    private void LisaaHissi(Vector paikka, double leveys, double korkeus, Image kuva, string tagi)
    {
        Hissi hissi = new Hissi(leveys*5,korkeus*3, Hissi1, paikka, paikka + new Vector(0, -500));
      
        hissi.Position = paikka;
        hissi.Image = kuva;
        hissi.Tag = tagi;
        Add(hissi);
        hissit.Add(hissi);
    }
    


    private void LisaaMaali(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject maali = PhysicsObject.CreateStaticObject(leveys*8, korkeus*8);
        maali.Position = paikka;
        maali.Image = OvenKuva;
        maali.IgnoresGravity = true;
        maali.Y = paikka.Y + 80;
        maali.Tag = "maali";
        
        Add(maali, 1);
    }
    

    private void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        pelaaja1 = new PlatformCharacter(leveys*3, korkeus*3);
        pelaaja1.Position = paikka;
        pelaaja1.Mass = 5.0;
        pelaaja1.Image = pelaajanKuva;
        AddCollisionHandler(pelaaja1, "vesi" ,PelaajaKuolee);
        AddCollisionHandler(pelaaja1, "pomppu", PelaajaPomppaa);
        AddCollisionHandler(pelaaja1, "maali", PelaajaLapaisiKentan);
        AddCollisionHandler(pelaaja1, "avain", AvainLoytyi);
        Add(pelaaja1, 1);
    }

    private void LisaaNappaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, PaaValikko, "päävalikko");

        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, -NOPEUS);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, NOPEUS);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, hyppyNopeus);
        ControllerOne.Listen(Button.Back, ButtonState.Pressed, Exit, "Poistu pelistä");
        Keyboard.Listen(Key.E, ButtonState.Pressed, PainaaNappia, "paina nappia");
       // Keyboard.Listen(Key.E, ButtonState.Pressed, PainaaNappia2, "paina nappia");
        ControllerOne.Listen(Button.DPadLeft, ButtonState.Down, Liikuta, "Pelaaja liikkuu vasemmalle", pelaaja1,
            -NOPEUS);
        ControllerOne.Listen(Button.DPadRight, ButtonState.Down, Liikuta, "Pelaaja liikkuu oikealle", pelaaja1, NOPEUS);
        ControllerOne.Listen(Button.A, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, hyppyNopeus);
       

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
    }
    

    private void Liikuta(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Walk(nopeus);
    }

    private void Hyppaa(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Jump(nopeus);
    }

    void PelaajaPomppaa(PhysicsObject pelaaja1, PhysicsObject pompputaso)
    {
        Hyppaa((PlatformCharacter)pelaaja1, hyppyNopeus*1.5);
    }

   

    void PelaajaKuolee(PhysicsObject pelaaja1, PhysicsObject vesi)
    {
        pelaaja1.Destroy();
        SeuraavaKentta();
    }

    void AvainLoytyi(PhysicsObject pelaaja1, PhysicsObject avain)
    {
        Avainkeratty++;
        MessageDisplay.Add("You found a key!");
        avain.Destroy();
    }
    void PelaajaLapaisiKentan(PhysicsObject pelaaja1, PhysicsObject maali)
    {
        if (Avainkeratty == 3)
        {
            kenttaNro++;
            LapaisyValikko();
        }
        
    }
    void PainaaNappia()
    {
        foreach (var nappi in napit)
        {
            if (nappi.IsInsideRect(pelaaja1.Position))
            {
               nappi.hissi.Liikuta();
            }
        }
    }
}

class Hissi : PhysicsObject
{
    public Nappi nappi;
    public Vector sijainti1;
    public Vector sijainti2;
    public Hissi(double leveys, double korkeus, Image kuva, Vector sijainti1, Vector sijainti2) : base(leveys, korkeus)
    {
        this.Image = kuva;
        this.sijainti1 = sijainti1;
        this.sijainti2 = sijainti2;
        this.CanRotate = false;
        this.IgnoresGravity = true;
        this.Mass = 10000;
       

    }

    public void Liikuta()
    {
        if (Math.Abs(this.Position.X - sijainti1.X) < 2 && (Math.Abs(this.Position.Y - sijainti1.Y) < 2))
        {
            this.MoveTo(sijainti2, 150);
        }
        else
        {
            this.MoveTo(sijainti1, 150);
        }
        
    }
}
class Nappi : PhysicsObject
{
    public bool OnkoPainettu = false;
    public Hissi hissi;
   
    public Nappi(double leveys, double korkeus) : base(leveys, korkeus)
    {
        
    }

    public void Paina(Vector paikka)
    {
        hissi.Move(paikka);
    }
}