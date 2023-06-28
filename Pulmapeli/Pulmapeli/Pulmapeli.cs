using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using Vector = Jypeli.Vector;

namespace Pulmapeli;

public class Pulmapeli : PhysicsGame
{
    private const double NOPEUS = 200;
    private const double HYPPYNOPEUS = 750;
    private const int RUUDUN_KOKO = 40;

    private PlatformCharacter pelaaja1;
    int LiikkuvaTasoY;
    private int LiikkuvaTasoX;


    private int kenttaNro = 1;
    private List<PhysicsObject> napit = new List<PhysicsObject>();
    private List<PhysicsObject> hissit = new List<PhysicsObject>();
    private int NappiaPainettu = 0;

    private Image pelaajanKuva = LoadImage("norsu.png");
    private Image tahtiKuva = LoadImage("tahti.png");
    private Image NapinKuva = LoadImage("nappiLapinakyva.png");
    private Image TaustaKuva = LoadImage("taustakuva");
    private Image TasonKuva = LoadImage("Taso");
    private Image VesiKuva = LoadImage("vesi");
    private Image LiikkuvanTasonKuva = LoadImage("LiikkuvaTaso");
    private Image PomppuTasonKuva = LoadImage("pompputaso");
    private SoundEffect MaaliAani = LoadSoundEffect("maali.wav");

    

    private PhysicsObject LiikkuvaTaso;
    private PhysicsObject nappi;

    public override void Begin()
    {
        Gravity = new Vector(0, -1000);
        PaaValikko();
        LuoKentta($"kentta{kenttaNro}");
        LisaaNappaimet();

        Camera.Follow(pelaaja1);
        Camera.ZoomFactor = 0.5;
        Camera.StayInLevel = true;

        MasterVolume = 0.5;
    }

    private void LuoKentta(string kenttanro)
    {
        TileMap kentta = TileMap.FromLevelAsset(kenttanro);
        kentta.SetTileMethod('#', LisaaTaso);
        kentta.SetTileMethod('*', LisaaTahti);
        kentta.SetTileMethod('P', LisaaPelaaja);
        kentta.SetTileMethod('n', LisaaNappi);
        kentta.SetTileMethod('=', LisaaLiikkuvaTaso);
        kentta.SetTileMethod('T', LisaaVesi);
        kentta.SetTileMethod('p', LisaaPomppuTaso);
        
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);
        Level.Background.Image = TaustaKuva;
        Level.Background.ScaleToLevelFull();
        
    }

    void SeuraavaKentta()
    {
        ClearAll();
        if (kenttaNro > 3) Exit();
        LuoKentta($"kentta{kenttaNro}");
        

    }

  

    void PaaValikko()
    {
        MultiSelectWindow paaValikko = new MultiSelectWindow("Main menu", "Level 1", "Level 2", "Level 3", "Level 4", "Exit"); 
        Add(paaValikko);
        paaValikko.AddItemHandler(0, Kentta);
        paaValikko.AddItemHandler(1, Kentta);
        paaValikko.AddItemHandler(2, Kentta);
        paaValikko.AddItemHandler(3, Kentta);
        paaValikko.AddItemHandler(4, Exit);
        PushButton[] painikkeet = paaValikko.Buttons;
        
        void Kentta(List<int> painikkeet)
        {
            kenttaNro = painikkeet.Count;
        }
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
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Image = TasonKuva;
        Add(taso);
    }

    void LisaaPomppuTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject PomppuTaso = PhysicsObject.CreateStaticObject(leveys*4, korkeus*2);
        PomppuTaso.Position = paikka;
        PomppuTaso.Image = PomppuTasonKuva;
     
        PomppuTaso.Tag = "pomppu";
        Add(PomppuTaso);
    }
    private void LisaaLiikkuvaTaso(Vector paikka, double leveys, double korkeus)
    {
        LiikkuvaTaso = PhysicsObject.CreateStaticObject(leveys*4, korkeus*2);
        LiikkuvaTaso.Position = paikka;
        LiikkuvaTaso.Image = LiikkuvanTasonKuva;
        Add(LiikkuvaTaso);
        hissit.Add(LiikkuvaTaso);
    }

    private void LisaaTahti(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject tahti = PhysicsObject.CreateStaticObject(leveys, korkeus);
        tahti.IgnoresCollisionResponse = true;
        tahti.Position = paikka;
        tahti.Image = tahtiKuva;
        tahti.Tag = "tahti";
        Add(tahti);
    }

    private void LisaaNappi(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject nappi = new PhysicsObject(leveys*3,korkeus*3);
        nappi.Image = NapinKuva;
        nappi.Position = paikka;
        nappi.IgnoresCollisionResponse = true;
        nappi.IgnoresGravity = true;
        nappi.Tag = "nappi";
        Add(nappi);
        napit.Add(nappi);

    }
    

    private void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        pelaaja1 = new PlatformCharacter(leveys*3, korkeus*3);
        pelaaja1.Position = paikka;
        pelaaja1.Mass = 5.0;
        pelaaja1.Image = pelaajanKuva;
        AddCollisionHandler(pelaaja1, "vesi" ,PelaajaKuolee);
        AddCollisionHandler(pelaaja1, "pomppu", PelaajaPomppaa);
        Add(pelaaja1, 1);
    }

    private void LisaaNappaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, -NOPEUS);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, NOPEUS);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, HYPPYNOPEUS);
        ControllerOne.Listen(Button.Back, ButtonState.Pressed, Exit, "Poistu pelistä");
        Keyboard.Listen(Key.E, ButtonState.Pressed, PainaaNappia, "paina nappia");
        ControllerOne.Listen(Button.DPadLeft, ButtonState.Down, Liikuta, "Pelaaja liikkuu vasemmalle", pelaaja1,
            -NOPEUS);
        ControllerOne.Listen(Button.DPadRight, ButtonState.Down, Liikuta, "Pelaaja liikkuu oikealle", pelaaja1, NOPEUS);
        ControllerOne.Listen(Button.A, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, HYPPYNOPEUS);
       

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
        Hyppaa((PlatformCharacter)pelaaja1, HYPPYNOPEUS*1.5);
    }

   

    void PelaajaKuolee(PhysicsObject pelaaja1, PhysicsObject vesi)
    {
        pelaaja1.Destroy();
        SeuraavaKentta();
    }
    void PainaaNappia()
    {

       

        Vector reuna = pelaaja1.Position;
        PhysicsObject nappiKohdalla = null;
        for (int i = 0; i < napit.Count; i++)
        {
            if (napit[i].IsInside(reuna))
            {
                nappiKohdalla = napit[i];
            }
        }

        if ((nappiKohdalla != null) && (NappiaPainettu == 0))
        {
            LiikkuvaTaso.Y = LiikkuvaTasoY - 250;
            NappiaPainettu = 1;
        }

        if ((nappiKohdalla != null) && (NappiaPainettu == 1))
        {
            LiikkuvaTaso.Y = LiikkuvaTasoY + 250;
            NappiaPainettu = 0;
        }
        
     

       
    }
}