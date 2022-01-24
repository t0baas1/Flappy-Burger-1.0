using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;


/// @author  Niko Kaleton, Topi Kanninen
/// @version 13.12.2021
public class BurgirBoi : PhysicsGame
{
   private PhysicsObject pelaaja;
   private PhysicsObject alaReuna;
   private PhysicsObject ylaReuna;
   private PhysicsObject alaseina;
   private PhysicsObject ylaseina;
   private PhysicsObject burgeri;

   private Image taustakuva = LoadImage("burger-king-logo");
   private Image burgerikuva = LoadImage("burgeri");

   private List<SoundEffect> aanet = new List<SoundEffect>();

   private int GravityY = -100;
   private int GravityX = 600;

   private int kerroin = 0;

   IntMeter pisteet;
   EasyHighScore parhaat = new EasyHighScore(9);
   private Vector portinpaikka = new Vector(0, 0);


    public override void Begin()
    {
        AlustaAanet();
        aanet[0].Play();

        MediaPlayer.Play("taustamatto");
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 0.5;

        LisaaAlkuvalikko(null);
    }


    public void AlustaAanet()
    {
        SoundEffect x = LoadSoundEffect("nomnom");
        aanet.Add(x);

        SoundEffect y = LoadSoundEffect("crunch");
        aanet.Add(y);

    }

    public void LisaaAlkuvalikko( Window sender)
    {
        parhaat.Show();
        parhaat.HighScoreWindow.X = 200;

        MultiSelectWindow alkuvalikko = new MultiSelectWindow("Pelin alkuvalikko", "Aloita peli", "Lopeta");
        alkuvalikko.X = -200;
        Add(alkuvalikko);
        alkuvalikko.AddItemHandler(0, AloitaPeli);
        alkuvalikko.AddItemHandler(1, Exit);
        alkuvalikko.DefaultCancel = 2;
    }


    private void AloitaPeli()
    {
        ClearAll();
        LuoKentta();
        LuoAjastimet();
        LuoPelaaja(this, 100, 100);
 

        LisaaPisteLaskuri();

        Camera.Follow(pelaaja);
        Camera.FollowOffset = new Vector(Screen.Width / 3, 0.0);

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Space, ButtonState.Pressed, MoveUp, "Liikuta ylös");
        Keyboard.Listen(Key.Space, ButtonState.Released, ResetGravity, "Liikuta ylös");
        Keyboard.Listen(Key.K, ButtonState.Pressed, LuoSeina, "Liikuta ylös");
    }


    /// <summary>
    /// Luodaan kenttä sekä pelaaja
    /// </summary>
    private void LuoKentta()
    {
        // Luodaan ala- ja ylärajat pelikentälle.
        Level.Width = 1000000;
        alaReuna = Level.CreateBottomBorder();
        alaReuna.Tag = "alareuna";
        AddCollisionHandler(alaReuna, "pelaaja", Seinaosuma);
        ylaReuna = Level.CreateTopBorder();
        ylaReuna.Tag = "ylareuna";
        AddCollisionHandler(ylaReuna, "pelaaja", Seinaosuma);


        /// Painovoima
        Gravity = new Vector(GravityX, GravityY);
    }

    
    /// <summary>
    /// Aliohjelma mikä luo pelaajan
    /// </summary>
    private void LuoPelaaja(PhysicsGame peli, double w, double h)
    {
        pelaaja = new PhysicsObject(w, h);
        pelaaja.Shape = Shape.Circle;
        pelaaja.Image = LoadImage("tuksu");
        pelaaja.MomentOfInertia = 9000;
        pelaaja.Tag = "pelaaja";
        Add(pelaaja);
    }


    /// <summary>
    /// Pelin ajastimien lisäys
    /// </summary>
    private void LuoAjastimet()
    {
        Timer ajastinSeinat = new Timer();
        ajastinSeinat.Interval = 2;
        ajastinSeinat.Timeout += delegate { LuoSeina(); };
        ajastinSeinat.Start();

        Timer tausta = new Timer();
        tausta.Interval = 3;
        tausta.Timeout += delegate { LuoTausta(); };
        tausta.Start();
    } 
     

    /// <summary>
    /// Luodaan kentälle tausta. Haetaan taustaksi kuva hakemistosta.
    /// </summary>
    private void LuoTausta()
    {
        GameObject tausta = new GameObject(900, 500);
        tausta.Image = taustakuva;
        tausta.X = pelaaja.Position.X + 1200;
        Add(tausta, -1);
    }


    /// <summary>
    /// Lisää vaikeutta peliin kun pisteet kasvavat. Vaikeutus tapahtuu porttien väliä pienentämällä.
    /// </summary>
    private void Vaikeutus()
    {
        while (kerroin <= 450)
        {
           kerroin += 50;
           break;
        }
    }


    /// <summary>
    /// Seinien luonti
    /// </summary>
    private void LuoSeina()
    {
        alaseina = new PhysicsObject(100,200);
        alaseina.X = pelaaja.X + 1200;
        alaseina.X += RandomGen.NextInt(-200, 200);
        alaseina.Y = Level.Bottom-10;
        alaseina.IgnoresGravity = true;
        alaseina.Height = RandomGen.NextDouble(0, Level.Height);
        alaseina.Width = 100;
        alaseina.MakeStatic();
        alaseina.Tag = "seina";
        AddCollisionHandler(alaseina, "pelaaja", Seinaosuma);
        Add(alaseina,1);

        ylaseina = new PhysicsObject(100,200);
        ylaseina.X = pelaaja.X + 1200;
        ylaseina.X += RandomGen.NextInt(-200, 200);
        ylaseina.Y = Level.Top;
        ylaseina.IgnoresGravity = true;
        ylaseina.Height = Level.Height - (alaseina.Height) + kerroin;
        ylaseina.Width = 100;
        ylaseina.MakeStatic();
        ylaseina.Tag = "seina";
        AddCollisionHandler(ylaseina, "pelaaja", Seinaosuma);
        Add(ylaseina,1);

        portinpaikka.X = alaseina.X;
        portinpaikka.Y = alaseina.Y;

        LuoBurgeri(new Vector( portinpaikka.X - 200, portinpaikka.Y));

        LuoBurgeri(new Vector (portinpaikka.X + 200, portinpaikka.Y));
    }


    /// <summary>
    /// Käsitellään pelaajan osuma seinään
    /// </summary>
    /// <param name="seina">Osuttu kohde</param>
    /// <param name="pelaaja">Pelaaja</param>
    private void Seinaosuma(PhysicsObject seina, PhysicsObject pelaaja)
    {
        TuhoaPelaaja(pelaaja);
        LisaaAlkuvalikko(null);  
    }


    /// <summary>
    /// Käsitellään pelaajan osuman vaikutus
    /// </summary>
    /// <param name="pelaaja">Pelaaja</param>
    private void TuhoaPelaaja(PhysicsObject pelaaja)
    {
        Explosion rajahdys = new Explosion(pelaaja.Width * 10);
        rajahdys.Position = pelaaja.Position;
        rajahdys.UseShockWave = false;
        Add(rajahdys);
        Remove(pelaaja);
        parhaat.EnterAndShow(pisteet.Value);
    }


    /// <summary>
    /// Luo burgereita kentälle. Burgerit tuhoutuvat 10 s päästä syntymästä
    /// </summary>
    private void LuoBurgeri(Vector sijainti)
    {
        burgeri = new PhysicsObject(70, 70, Shape.Circle);
        burgeri.X = sijainti.X + 50;
        burgeri.Mass = 0;
        burgeri.IgnoresGravity = true;
        burgeri.MakeStatic();
        burgeri.IgnoresCollisionResponse = true;
        burgeri.Image = burgerikuva;
        burgeri.Y = RandomGen.NextDouble(-300, 300);
        burgeri.LifetimeLeft = TimeSpan.FromSeconds(10.0);
        burgeri.Tag = "burgeri";
        AddCollisionHandler(burgeri, "pelaaja", Burgeriosuma);
        Add(burgeri);
    }


    /// <summary>
    /// Käsitellään pelaajan osuma burgeriin
    /// </summary>
    /// <param name="burgeri">Burgeri johon on osuttu</param>
    /// <param name="pelaaja">Pelaaja</param>
    private void Burgeriosuma(PhysicsObject burgeri, PhysicsObject pelaaja)
    {
        TuhoaBurgeri(burgeri);
    }


    /// <summary>
    /// Käsitellään burgeriosuman vaikutus
    /// </summary>
    /// <param name="burgeri">Burgeri johon pelaaja on osunut</param>
    private void TuhoaBurgeri(PhysicsObject burgeri)
    {
        pisteet.Value++;
        if (pisteet.Value % 10 == 0) Vaikeutus();
        int nro = RandomGen.NextInt(0, aanet.Count);
        aanet[nro].Play();
        Remove(burgeri);
    }
    

    /// <summary>
    /// Palauttaa painovoiman ylöspäin.
    /// </summary>
    private void MoveUp()
    {
        Gravity = new Vector(0, 800);
    }


    /// <summary>
    /// Palauttaa painovoiman alaspäin
    /// </summary>
    private void ResetGravity()
    {
        Gravity = new Vector(0, -800);
    }


    /// <summary>
    /// Pistelaskurin luonti
    /// </summary>
    private void LisaaPisteLaskuri()
    {
        pisteet = new IntMeter(0);
        Label naytto = new Label();
        naytto.BindTo(pisteet);
        naytto.X = Screen.Width / -2.4;
        naytto.Y = Screen.Height / 2.4; //Level.Left + 200;
        naytto.TextColor = Color.Black;
        naytto.BorderColor = Color.White;
        naytto.Color = Color.White;
        Add(naytto);
    }
}
