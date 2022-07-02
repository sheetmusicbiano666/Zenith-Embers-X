using System;
using System.Collections.Generic;
using ScriptedEngine;
using ZenithEngine;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using System.Threading;
using System.Threading.Tasks;
using OpenTK;
using System.Linq;

//---------Enums---------
    #region Enums

enum NoteStyle
{
    S9,
    S10,
    Glow,
}

enum KeyboardStyle
{
    S7,
    S8,
    S9,
    Glow,
}

enum SparkleType
{
    Normal,
    Rounded,
    Flame,
}

enum BarType
{
    Piano,
    White,
}

/*enum MiddleC
{
    C3,
    C4,
    C5,
}*/

#endregion

//---------Particle Classes---------

    #region Particle Classes

abstract class Particle
{
    public double life = 0;

    public abstract void Step(double delta);
}

class KeyHazeParticle : Particle
{
    static double maxLife = 0.2;

    public int key;

    public double brightness = 0;

    public Vector2d pos;

    public KeyHazeParticle(int key, Random r)
    {
        this.key = key;
        life = maxLife;
        pos = new Vector2d(r.NextDouble(), r.NextDouble());
    }

    public override void Step(double delta)
    {
        life -= delta;

        brightness = 1 - Math.Abs((life * 2 - maxLife) / maxLife);
    }
}

class KeySparkParticle : Particle
{
    static double maxLife = 0.2;
    static double minSize = 0.7;

    public int key;

    public double brightness = 0;

    public Vector2d pos;

    public double rotation;
    public double size;
    public bool flipped;

    public KeySparkParticle(int key, Random r)
    {
        this.key = key;
        life = maxLife;
        pos = new Vector2d(r.NextDouble(), r.NextDouble());
        rotation = r.NextDouble() * 2 - 1;
        flipped = r.NextDouble() < 0.5;
    }

    public override void Step(double delta)
    {
        life -= delta;

        brightness = 1 - Math.Abs((life * 2 - maxLife) / maxLife);
        size = brightness * (1 - minSize) + minSize;
    }
}

class KeyDebrisParticle : Particle
{
    public Vector2d pos;
    public Vector2d vel;

    public double rotation;
    public double size;

    public int key;

    public KeyDebrisParticle(int key, KeyLayout layout, Random r)
    {
        this.key = key;

        pos = new Vector2d(0, 0);
        vel = new Vector2d((r.NextDouble() - 0.5) * 0.01, r.NextDouble() * 0.003);
        life = 0.3;
        rotation = r.NextDouble() * 2 - 1;
        size = r.NextDouble() * 0.5 + 0.2;
    }

    public override void Step(double delta)
    {
        life -= delta;

        pos += vel * delta * 60;
        if (pos.Y < 0) life = 0;
        vel.Y -= 0.001 * delta * 60;
    }
}

#endregion

//---------Script Settings---------
    public class Script
    {
    public string Description = "Embers X";
    public string Preview = "Preview/Preview.png";

    public long LastNoteCount = 0;
    public double NoteCollectorOffset { get { return -maxCapTicks; } }
    public double NoteScreenTime { get { return noteScreenTime + maxCapTicks; } }
    double maxCapTicks = 0;
    double noteScreenTime = 0;

    public bool UseProfiles = true;

    Random r = new Random();

//---------Textures---------

    #region Textures

    // ------------ Syn10 ------------

    //Notes
    Texture s10NoteCapTop = IO.LoadTexture("Notes/Synthesia 10/noteTop.png");
    Texture s10NoteCapBottom = IO.LoadTexture("Notes/Synthesia 10/noteBottom.png");
    Texture s10NoteBody = IO.LoadTexture("Notes/Synthesia 10/note.png");

    // ------------ Syn9 ------------

    //KB
    Texture s9Bar = IO.LoadTexture("Keyboards/Synthesia 9/bar.png");
    Texture s9KeyBlack = IO.LoadTexture("Keyboards/Synthesia 9/blackKeys.png");
    Texture s9KeyBlackPressed = IO.LoadTexture("Keyboards/Synthesia 9/blackKeysPressed.png");
    Texture s9KeyWhite = IO.LoadTexture("Keyboards/Synthesia 9/whiteKeys.png");
    Texture s9KeyWhitePressed = IO.LoadTexture("Keyboards/Synthesia 9/whiteKeysPressed.png");
    Texture s9KeyWhiteWhole = IO.LoadTexture("Keyboards/Synthesia 9/whiteKeyWhole.png");
    Texture s9KeyWhiteWholePressed = IO.LoadTexture("Keyboards/Synthesia 9/whiteKeyWholePressed.png");

    //Notes
    Texture s9NoteBlackCapTop = IO.LoadTexture("Notes/Synthesia 9/noteTopBlack.png");
    Texture s9NoteBlackCapBottom = IO.LoadTexture("Notes/Synthesia 9/noteBottomBlack.png");
    Texture s9NoteBlackBody = IO.LoadTexture("Notes/Synthesia 9/noteBlack.png");

    Texture s9NoteWhiteCapTop = IO.LoadTexture("Notes/Synthesia 9/noteTopWhite.png");
    Texture s9NoteWhiteCapBottom = IO.LoadTexture("Notes/Synthesia 9/noteBottomWhite.png");
    Texture s9NoteWhiteBody = IO.LoadTexture("Notes/Synthesia 9/noteWhite.png");

    //Shadow
    Texture s9ShadowLarge = IO.LoadTexture("Keyboards/Synthesia 9/shadowLarge.png");
    Texture s9ShadowUnpressed = IO.LoadTexture("Keyboards/Synthesia 9/shadowUnpressed.png");
    Texture s9ShadowPressed = IO.LoadTexture("Keyboards/Synthesia 9/shadowPressed.png");

    // ------------ Syn9 Alt ------------

    Texture s9aKeyWhite = IO.LoadTexture("Keyboards/Synthesia 9 Alt/whiteKeys.png");
    Texture s9aKeyWhitePressed = IO.LoadTexture("Keyboards/Synthesia 9 Alt/whiteKeysPressed.png");
    Texture s9aKeyWhiteWhole = IO.LoadTexture("Keyboards/Synthesia 9 Alt/whiteKeyWhole.png");

    // ------------ Syn8 ------------

    //KB
    Texture s8Bar = IO.LoadTexture("Keyboards/Synthesia 8/bar.png");
    Texture s8KeyBlack = IO.LoadTexture("Keyboards/Synthesia 7/blackKey.png");
    Texture s8KeyBlackPressed = IO.LoadTexture("Keyboards/Synthesia 7/blackKeyPressed.png");
    Texture s8KeyWhite = IO.LoadTexture("Keyboards/Synthesia 8/whiteKeys.png");
    Texture s8KeyWhitePressed = IO.LoadTexture("Keyboards/Synthesia 8/whiteKeysPressed.png");
    Texture s8KeyWhiteWhole = IO.LoadTexture("Keyboards/Synthesia 8/whiteKeyWhole.png");
    Texture s8KeyWhiteWholePressed = IO.LoadTexture("Keyboards/Synthesia 8/whiteKeyWholePressed.png");

    // ------------ Syn7 ------------

    //KB
    Texture s7Bar = IO.LoadTexture("Keyboards/Synthesia 7/bar.png");
    Texture s7KeyBlack = IO.LoadTexture("Keyboards/Synthesia 7/blackKey.png");
    Texture s7KeyBlackPressed = IO.LoadTexture("Keyboards/Synthesia 7/blackKeyPressed.png");
    Texture s7KeyWhite = IO.LoadTexture("Keyboards/Synthesia 7/whiteKeys.png");
    Texture s7KeyWhitePressed = IO.LoadTexture("Keyboards/Synthesia 7/whiteKeysPressed.png");
    Texture s7KeyWhiteWhole = IO.LoadTexture("Keyboards/Synthesia 7/whiteKeyWhole.png");
    Texture s7KeyWhiteWholePressed = IO.LoadTexture("Keyboards/Synthesia 7/whiteKeyWholePressed.png");

    // ------------ Glow ------------

    //KB
    Texture glowKeyWhite = IO.LoadTexture("Keyboards/Glow/whiteKeys.png");
    Texture glowKeyWhitePressed = IO.LoadTexture("Keyboards/Glow/whiteKeysPressed.png");
    Texture glowKeyWhiteWhole = IO.LoadTexture("Keyboards/Glow/whiteKeyWhole.png");
    Texture glowKeyWhiteWholePressed = IO.LoadTexture("Keyboards/Glow/whiteKeyWholePressed.png");
    Texture glowKeyBlack = IO.LoadTexture("Keyboards/Glow/blackKeys.png");
    Texture glowKeyBlackPressed = IO.LoadTexture("Keyboards/Glow/blackKeysPressed.png");

    Texture glowBar = IO.LoadTexture("Keyboards/Glow/bar.png");

    //Notes
    Texture glowNoteCapTop = IO.LoadTexture("Notes/Glow/noteTop.png");
    Texture glowNoteCapBottom = IO.LoadTexture("Notes/Glow/noteBottom.png");
    Texture glowNoteBody = IO.LoadTexture("Notes/Glow/note.png");

    // ------------ Glow Alt ------------

    Texture glowaKeyWhite = IO.LoadTexture("Keyboards/Glow Alt/whiteKeys.png");
    Texture glowaKeyWhitePressed = IO.LoadTexture("Keyboards/Glow Alt/whiteKeysPressed.png");
    Texture glowaKeyWhiteWhole = IO.LoadTexture("Keyboards/Glow Alt/whiteKeyWhole.png");
    Texture glowaKeyWhiteWholePressed = IO.LoadTexture("Keyboards/Glow Alt/whiteKeyWholePressed.png");

    // ------------ Other ------------

    Texture prtKeyHaze = IO.LoadTexture("Particles/Synthesia/keyHaze.png");
    Texture prtKeySpark = IO.LoadTexture("Particles/Synthesia/keySpark.png");
    Texture prtKeyDebris = IO.LoadTexture("Particles/Synthesia/keyDebris.png");
    Texture prtKeySparkRnd = IO.LoadTexture("Particles/Custom/keySpark.png");

    Texture prtFlame = IO.LoadTexture("Particles/Custom/flame.png");

    Texture noteFlare = IO.LoadTexture("Particles/Synthesia/noteFlare.png");

    #endregion
//---------Settings UI---------

    List<LinkedList<Particle>> fullParticlesArray = new List<LinkedList<Particle>>();

    LinkedList<Particle>[] keyHazeParticles = new LinkedList<Particle>[256];
    LinkedList<Particle>[] keySparkParticles = new LinkedList<Particle>[256];
    LinkedList<Particle> keyDebrisParticles = new LinkedList<Particle>();

    UIDropdown uiVersion = new UIDropdown("SYNTHESIA STYLE", new[] { "Synthesia 10", "Synthesia 9", "Neon", "Synthesia 8"}); //Synthesia 7 code exists so if you want to use it just add "Synthesia 7" to this list
    UICheckbox uiOverlay = new UICheckbox("Octave Labels", true);
    UICheckbox uiOverlayn1 = new UICheckbox("Set C4 as Middle", false) { Padding = 7 };
    //UIDropdown uiMiddleC = new UIDropdown("Middle C", new[] { "C4", "C5", "C3"});
    UINumberSlider uiOverlayScale = new UINumberSlider("Labels Size", 0.8, 0, 1, 0, 1, 2);
    UINumberSlider uiKeyboardScale = new UINumberSlider("Keyboard Height", 1, 0.9, 1, 0.9, 1, 2);
    UINumberSlider uiKeyboardWhiteness = new UINumberSlider("Keyboard Whiteness", 0.8, 0.8, 1, 0.8, 1, 2);
    UICheckbox uiKeySparkle = new UICheckbox("Enabled", true);
    UICheckbox uiKeySparkleReset = new UICheckbox("Trigger Spark On New Note", false);
    UICheckbox uiExtendedKeyboard = new UICheckbox("Flat Keyboard (Synthesia 9 ,10 and Neon)", false) { Padding = 20 };
    UICheckbox uiRenderBackground = new UICheckbox("Display Background", true) { Padding = 9 };

    UICheckbox uiCredit = new UICheckbox("Credit Zenith", false);

    UIDropdown uiSparkleType = new UIDropdown("Sparkle Texture", new[] { "Normal", "Rounded", "Flame"});
    UIDropdown uiBarType = new UIDropdown("Bar Style", new[] { "Piano", "White"});

    UINumberSlider uiBackgroundTransparency = new UINumberSlider("Background Transparency", 1, 0, 1, 0, 1, 2);
    UINumberSlider uiBackgroundLineTransparency = new UINumberSlider("Line Transparency", 1, 0, 1, 0, 1, 2);

    UICheckbox uiRenderKeyFlare = new UICheckbox("Enabled", false) { Padding = 9 };
    UINumberSlider uiKeyFlareAttack = new UINumberSlider("Attack Time", 0.1, 0, 1, 0, 1, 2);
    UINumberSlider uiKeyFlareRelease = new UINumberSlider("Release Time", 0.2, 0, 1, 0, 1, 2);
    UINumberSlider uiKeyFlareHeight = new UINumberSlider("Height", 20, 0, 100, 0, 1000, 2);
    UINumberSlider uiKeyFlareStrength = new UINumberSlider("Strength", 0.3, 0, 1, 0, 1, 2);
    UINumberSlider uiKeyFlareStrengthDown = new UINumberSlider("Strength Below", 0, 0, 1, 0, 1, 2);

    UICheckbox uiRenderLayerNotes = new UICheckbox("Notes", true) { Padding = 5 };
    UICheckbox uiRenderLayerKeyboard = new UICheckbox("Keyboard", true) { Padding = 5 };
    UICheckbox uiRenderLayerFlares = new UICheckbox("Flares", true) { Padding = 5 };
    UICheckbox uiRenderLayerSparkle = new UICheckbox("Sparkle", true) { Padding = 5 };
    UICheckbox uiRenderLayerSparkleGlow = new UICheckbox("Sparkle Glow", true) { Padding = 5 };
    List<UICheckbox> uiRenderParticleLayers = new List<UICheckbox>();

    UINumberSlider uiFlareLightness = new UINumberSlider("Flare Lightness", 0.9, 0, 1, 0, 1, 2);
    UINumberSlider uiSparkleLightness = new UINumberSlider("Sparkle Lightness", 0.8, 0, 1, 0, 1, 2);
    UINumberSlider uiSparkleGlowLightness = new UINumberSlider("Sparkle Glow Intensity", 0.6, 0, 1, 0, 1, 2);

    public UISetting[] SettingsUI;

    Font cFont = IO.LoadFont(70, "Trebuchet MS", "C0123456789-");
    Font creditFont = IO.LoadFont(70, "Metropolis", "Zenith");

    Note[] keyHitNotes = new Note[256];
    Note[] keyHitNotesPrevious = new Note[256];

    ParticleSystem[] particles = new ParticleSystem[8];

    double[] keyFlareStrength = new double[257];
    double[] keyStrength = new double[257];

    public void Load()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i] = new ParticleSystem();
            uiRenderParticleLayers.Add(new UICheckbox("Particle system " + (i + 1), true) { Padding = 5 });
        }

        for (int i = 0; i < 256; i++)
        {
            keyHazeParticles[i] = new LinkedList<Particle>();
            fullParticlesArray.Add(keyHazeParticles[i]);
            keySparkParticles[i] = new LinkedList<Particle>();
            fullParticlesArray.Add(keySparkParticles[i]);
        }

        fullParticlesArray.Add(keyDebrisParticles);

        uiVersion.IndexChanged += i =>
        {
            uiExtendedKeyboard.Enabled = i < 3;
            uiOverlay.Enabled = i < 2;
            uiOverlayn1.Enabled = i < 2;
            uiOverlayScale.Enabled = i < 2;
        };

        uiRenderBackground.ValueChanged += i =>
        {
            uiBackgroundLineTransparency.Enabled = i;
            uiBackgroundTransparency.Enabled = i;
        };

        uiOverlay.ValueChanged += i =>
        {
            uiOverlayn1.Enabled = i;
            uiOverlayScale.Enabled = i;
        };

        Dictionary<string, IEnumerable<UISetting>> systemsTabs = new Dictionary<string, IEnumerable<UISetting>>();
        for (int i = 0; i < particles.Length; i++)
        {
            systemsTabs.Add("System " + (i + 1), particles[i].GetSettings(particles));
        }


        //---------License Check---------

        int synthesiaLicenseStatus = 1;
        string fileName = @"%appdata%\Synthesia\settings.xml";
        //IEnumerable<string> lines = File.ReadLines(fileName);

        if (synthesiaLicenseStatus == 1)
        {
            SettingsUI = new[] {
                new UITabs(new Dictionary<string, IEnumerable<UISetting>>() {
                   {"Main",
                        new UISetting[] {
                           new UILabel("General Settings", 20),
                            uiVersion,
                            uiExtendedKeyboard,
                            //uiCredit,
                        }
                    },
                    {"Keyboard",
                        new UISetting[] {
                            new UILabel("Keyboard Settings", 20),
                            uiKeyboardScale,
                            uiKeyboardWhiteness,
                            uiOverlay,
                            uiOverlayn1,
                            uiOverlayScale,
                        }
                    },
                    {"Background",
                        new UISetting[] {
                            new UILabel("Background Settings", 20),
                            uiRenderBackground,
                            uiBackgroundTransparency,
                            uiBackgroundLineTransparency
                        }
                    },
                    {"Sparkle",
                        new UISetting[] {
                            new UILabel("Synthesia Note Spark", 20),
                            uiKeySparkle,
                            uiSparkleType,
                            uiKeySparkleReset,
                            uiSparkleLightness,
                            uiSparkleGlowLightness,
                        }
                    },
                    {"Flare",
                        new UISetting[] {
                            new UILabel("Render Key Flare", 20),
                            uiRenderKeyFlare,
                            uiFlareLightness,
                            uiKeyFlareAttack,
                            uiKeyFlareRelease,
                            uiKeyFlareStrength,
                            uiKeyFlareHeight,
                        }
                    },
                    {"Layers",
                        new UISetting[] {
                            new UILabel("Toggle layers on/off", 20),
                            uiRenderLayerNotes,
                            uiRenderLayerKeyboard,
                            uiRenderLayerFlares,
                            uiRenderLayerSparkle,
                            uiRenderLayerSparkleGlow,
                        }.Concat(uiRenderParticleLayers).ToArray()
                    },
                    {"Particle Systems", new[] { new UITabs(systemsTabs) } },
                    {"About",
                        new UISetting[] {
                            new UILabel("Synthesia X - Zenith Scripted Pack", 26),
                            new UILabel("______________________________", 10),
                            new UILabel("Code by Arduano", 20),
                            new UILabel("Design and Textures by MBMS", 17),
                            new UILabel("______________________________", 10),
                            new UILabel("Please consider crediting Zenith and the Synthesia X pack in your video description.", 16),
                            new UILabel("Example text: Video made with Zenith using the Synthesia X pack", 16),
                            new UILabel("______________________________", 10),
                            new UILabel("Visit the official webpage for more info at: mbms.me/software/synthesiax", 16),
                        }
                    },
                })
            };
        }
        else{
            SettingsUI = new[] {
                new UITabs(new Dictionary<string, IEnumerable<UISetting>>() {
                   {"Pay",
                        new UISetting[] {
                            new UILabel("You need to purchase a copy of Synthesia to use this script.", 26),
                            new UILabel("Head over to synthesia.app to buy a copy of Synthesia.", 20)
                        }
                    }
                })
            };
        }
    }
//---------Variables---------

    public void Render(IEnumerable<Note> notes, RenderOptions options)
    {
        #region Misc Variables

        noteScreenTime = options.noteScreenTime;

        var noteStyle = NoteStyle.S9;
        var keyboardStyle = KeyboardStyle.S9;
        var keyboardS9Alt = uiExtendedKeyboard.Checked;
        var keyboardGlowAlt = uiExtendedKeyboard.Checked;
        switch (uiVersion.Value)
        {
            case "Synthesia 10":
                noteStyle = NoteStyle.S10;
                keyboardStyle = KeyboardStyle.S9;
                break;
            case "Synthesia 9":
                noteStyle = NoteStyle.S9;
                keyboardStyle = KeyboardStyle.S9;
                break;
            case "Synthesia 8":
                noteStyle = NoteStyle.S9;
                keyboardStyle = KeyboardStyle.S8;
                break;
            case "Synthesia 7":
                noteStyle = NoteStyle.S9;
                keyboardStyle = KeyboardStyle.S7;
                break;
            case "Neon":
                noteStyle = NoteStyle.Glow;
                keyboardStyle = KeyboardStyle.Glow;
                break;
        }

        var sparkleType = SparkleType.Normal;
        switch (uiSparkleType.Value)
        {
            case "Normal":
                sparkleType = SparkleType.Normal;
                break;
            case "Rounded":
                sparkleType = SparkleType.Rounded;
                break;
            case "Flame":
                sparkleType = SparkleType.Flame;
                break;
        }

        var barType = BarType.Piano;
        switch (uiBarType.Value)
        {
            case "Piano":
                barType = BarType.Piano;
                break;
            case "White":
                barType = BarType.White;
                break;
        }

        /*var middleC = MiddleC.C4;
        switch (uiSparkleType.Value)
        {
            case "C5":
                middleC = MiddleC.C5;
                break;
            case "C4":
                middleC = MiddleC.C4;
                break;
            case "C3":
                middleC = MiddleC.C3;
                break;
        }*/

        double keyboardHeight = 0.158 * uiKeyboardScale.Value;;
        if (keyboardStyle == KeyboardStyle.S8) keyboardHeight = 0.16;
        if (keyboardStyle == KeyboardStyle.S7) keyboardHeight = 0.165;



        double barHeight = 4.5 / 100;
        if (keyboardStyle == KeyboardStyle.S8) barHeight = 7.0 / 100;
        if (keyboardStyle == KeyboardStyle.S7) barHeight = 5.0 / 100;
        if (keyboardStyle == KeyboardStyle.Glow) barHeight = 2.0 / 100;

        keyboardHeight = keyboardHeight / (options.lastKey - options.firstKey) * 128;
        keyboardHeight = keyboardHeight / (1920.0 / 1080.0) * options.renderAspectRatio;

        double notePosFactor = 1 / options.noteScreenTime * (1 - keyboardHeight);
        double renderCutoff = options.midiTime + options.noteScreenTime;

        LastNoteCount = 0;

        var kbOptions = new KeyboardOptions();
        if (keyboardStyle == KeyboardStyle.S9)
        {
            kbOptions = new KeyboardOptions()
            {
                advancedBlackKeyOffsets = new[] { 0.02, -0.035, 0.0, -0.02, -0.03 },
                blackKeyScale = 0.63
            };
        }
        var layout = Util.GetKeyboardLayout(options.firstKey, options.lastKey, kbOptions);

        int firstKey = options.firstKey;
        int lastKey = options.lastKey;
        if (layout.blackKey[firstKey]) firstKey--;
        if (layout.blackKey[lastKey - 1]) lastKey++;

        var keyColors = new Color4[514];
        var keyPressed = new bool[257];
        var keyWeight = new int[257];

        for (int i = 0; i < 256; i++)
        {
            keyHitNotesPrevious[i] = keyHitNotes[i];
            keyHitNotes[i] = null;
        }

        Color4 whiteShade = new Color4(255, 255, 255, 255);

        double maxCapSize = 0;

        #endregion
//---------Background---------

        #region Background

        if (uiRenderBackground.Checked)
        {
            var lineWidth = 0.02;
            var lineHeight = lineWidth * options.renderAspectRatio;
            bool s10 = uiVersion.Value == "Synthesia 10";
            bool glow = uiVersion.Value == "Neon";
            byte bgAlpha = (byte)(uiBackgroundTransparency.Value * 255);
            byte lineAlpha = (byte)(uiBackgroundLineTransparency.Value * 255);
            if (s10)
                IO.RenderQuad(0, 1, 1, 0, new Color4(37, 37, 37, bgAlpha));
            else if (glow)
                IO.RenderQuad(0, 1, 1, 0, new Color4(10, 10, 10, bgAlpha));
            else
                IO.RenderQuad(0, 1, 1, 0, new Color4(48, 48, 48, bgAlpha));

            if (!options.midiTimeBased)
            {
                var div = options.midiBarLength;
                var start = options.midiTime;
                start = start - (start % div) + div;
                for (double h = start; h <= renderCutoff; h += div)
                {
                    double line = 1 - (renderCutoff - h) * notePosFactor;
                    var top = line + layout.whiteKeyWidth * lineHeight;
                    var bottom = line - layout.whiteKeyWidth * lineHeight;
                    if (s10)
                    {
                        IO.RenderQuad(0, top, 1, bottom, new Color4(50, 50, 50, lineAlpha));
                    }
                    else if (glow)
                    {
                        IO.RenderQuad(0, top, 1, bottom, new Color4(30, 30, 30, lineAlpha));
                    }
                    else
                    {
                        IO.RenderQuad(0, top, 1, bottom, new Color4(80, 80, 80, lineAlpha));
                    }
                }
            }
            for (int i = options.firstKey; i < options.lastKey; i++)
            {
                var line = layout.keys[i].left;
                var left = line - layout.whiteKeyWidth * lineWidth;
                var right = line + layout.whiteKeyWidth * lineWidth;
                if (s10)
                {
                    if (i % 12 == 0)
                    {
                        IO.RenderQuad(left, 1, right, 0, new Color4(80, 80, 80, lineAlpha));
                    }
                    if (i % 12 == 5)
                    {
                        IO.RenderQuad(left, 1, right, 0, new Color4(50, 50, 50, lineAlpha));
                    }
                }
                else if (glow)
                {
                    if (i % 12 == 0 || i % 12 == 5)
                    {
                        IO.RenderQuad(left, 1, right, 0, new Color4(30, 30, 30, lineAlpha));
                    }
                }
                else
                {
                    if (i % 12 == 0 || i % 12 == 5)
                    {
                        IO.RenderQuad(left, 1, right, 0, new Color4(80, 80, 80, lineAlpha));
                    }
                }
            }
        }

        #endregion
//---------Notes---------

        for (int i = 0; i < particles.Length; i++)
        {
            if (uiRenderParticleLayers[i].Checked)
            {
                particles[i].RenderUnderNotes(layout, keyboardHeight, options);
            }
        }

        #region Notes

        if (noteStyle == NoteStyle.S9)
        {
            IO.SelectTextureShader(TextureShaders.Hybrid);
        }
        else if (noteStyle == NoteStyle.Glow)
        {
            IO.SelectTextureShader(TextureShaders.Hybrid);
        }
        foreach (var note in Util.BlackNotesAbove(notes))
        {
            LastNoteCount++;

            double top = 1 - (renderCutoff - note.end) * notePosFactor;
            double bottom = 1 - (renderCutoff - note.start) * notePosFactor;
            double left = layout.keys[note.key].left;
            double right = layout.keys[note.key].right;
            if (!note.hasEnded) top = 2;

            var capBottomTex = s10NoteCapBottom;
            var capTopTex = s10NoteCapTop;
            var bodyTex = s10NoteBody;

            if (noteStyle == NoteStyle.S9)
            {
                if (layout.blackKey[note.key])
                {
                    capBottomTex = s9NoteBlackCapBottom;
                    capTopTex = s9NoteBlackCapTop;
                    bodyTex = s9NoteBlackBody;
                }
                else
                {
                    capBottomTex = s9NoteWhiteCapBottom;
                    capTopTex = s9NoteWhiteCapTop;
                    bodyTex = s9NoteWhiteBody;
                }
            }

            if (noteStyle == NoteStyle.Glow)
            {
                capBottomTex = glowNoteCapBottom;
                capTopTex = glowNoteCapTop;
                bodyTex = glowNoteBody;
            }

            double capHeight = (right - left) / capTopTex.aspectRatio * options.renderAspectRatio;
            if (maxCapSize < capHeight) maxCapSize = capHeight;
            double capExtra = 0;
            if (layout.blackKey[note.key] && noteStyle == NoteStyle.S10)
            {
                capExtra = capHeight * layout.whiteNoteWidth / layout.blackNoteWidth - capHeight;
            }
            capHeight += capExtra;
            double capTop = top - capHeight;
            double capBottom = bottom + capHeight;
            if (capTop < capBottom)
            {
                capTop = (capTop + capBottom) / 2;
                capBottom = capTop;
                top = capTop + capHeight;
                bottom = capBottom - capHeight;
            }
            capTop += capExtra;
            capBottom -= capExtra;

            if (top < keyboardHeight || bottom > 1) continue;

            Color4 leftCol = note.color.left;
            Color4 rightCol = note.color.right;

            if (layout.blackKey[note.key])
            {
                leftCol = Util.BlendColors(leftCol, new Color4(0, 0, 0, 0.3f));
                rightCol = Util.BlendColors(rightCol, new Color4(0, 0, 0, 0.3f));
            }

            if (uiRenderLayerNotes.Checked)
            {
                IO.RenderQuad(left, capTop, right, capBottom, leftCol, rightCol, rightCol, leftCol, bodyTex);
                IO.RenderQuad(left, top, right, capTop, leftCol, rightCol, rightCol, leftCol, capTopTex);
                IO.RenderQuad(left, capBottom, right, bottom, leftCol, rightCol, rightCol, leftCol, capBottomTex);
            }

            if (note.start < options.midiTime)
            {
                keyPressed[note.key] = true;
                keyHitNotes[note.key] = note;
                keyWeight[note.key]++;

                keyColors[note.key * 2] = Util.BlendColors(keyColors[note.key * 2], note.color.left);
                keyColors[note.key * 2 + 1] = Util.BlendColors(keyColors[note.key * 2 + 1], note.color.right);
            }
        }
        maxCapTicks = maxCapSize / (1 - keyboardHeight) * options.noteScreenTime;

        IO.SelectTextureShader(TextureShaders.Normal);

        #endregion
//---------Generate Particles---------

        #region Generate Particles

        for (int i = 0; i < 256; i++)
        {
            if (!keyPressed[i])
            {
                if (keyHazeParticles[i].Count > 0) keyHazeParticles[i].Clear();
                if (keySparkParticles[i].Count > 0) keySparkParticles[i].Clear();
            }
            else
            {
                if (uiKeySparkleReset.Checked && keyHitNotes[i] != keyHitNotesPrevious[i])
                {
                    if (keyHazeParticles[i].Count > 0) keyHazeParticles[i].Clear();
                    if (keySparkParticles[i].Count > 0) keySparkParticles[i].Clear();
                }
                else
                {
                    if (sparkleType == SparkleType.Normal)
                    {
                        if (uiKeySparkle.Checked)
                        {
                            if (r.NextDouble() < 0.5 && keyHazeParticles[i].Count < 20) keyHazeParticles[i].AddLast(new KeyHazeParticle(i, r));
                            if (r.NextDouble() < 0.5 && keySparkParticles[i].Count < 20) keySparkParticles[i].AddLast(new KeySparkParticle(i, r));

                            if (r.NextDouble() < 0.02 && keyDebrisParticles.Count < 200) keyDebrisParticles.AddLast(new KeyDebrisParticle(i, layout, r));
                        }
                    }
                    else if (sparkleType == SparkleType.Rounded)
                    {
                        if (uiKeySparkle.Checked)
                        {
                            if (r.NextDouble() < 0.8 && keyHazeParticles[i].Count < 20) keyHazeParticles[i].AddLast(new KeyHazeParticle(i, r));
                            if (r.NextDouble() < 0.8 && keySparkParticles[i].Count < 20) keySparkParticles[i].AddLast(new KeySparkParticle(i, r));

                            if (r.NextDouble() < 0.01 && keyDebrisParticles.Count < 200) keyDebrisParticles.AddLast(new KeyDebrisParticle(i, layout, r));
                        }
                    }
                    else if (sparkleType == SparkleType.Flame)
                    {
                        if (uiKeySparkle.Checked)
                        {
                            if (r.NextDouble() < 0.9 && keyHazeParticles[i].Count < 20) keyHazeParticles[i].AddLast(new KeyHazeParticle(i, r));
                            if (r.NextDouble() < 0.9 && keySparkParticles[i].Count < 20) keySparkParticles[i].AddLast(new KeySparkParticle(i, r));

                            if (r.NextDouble() < 0.01 && keyDebrisParticles.Count < 200) keyDebrisParticles.AddLast(new KeyDebrisParticle(i, layout, r));
                        }
                    }
                }
            }
        }

        #endregion

        foreach(var p in particles)
        {
            p.GeneratUnderNotes(notes, keyboardHeight, firstKey, lastKey, layout, options);
            p.GeneratOnKeys(keyColors, keyWeight, firstKey, lastKey, layout, options);
        }

        for (int i = 0; i < particles.Length; i++)
        {
            if (uiRenderParticleLayers[i].Checked)
            {
                particles[i].RenderUnderKeys(layout, keyboardHeight, options);
            }
        }
//---------Sparkle---------

        if (uiRenderLayerSparkle.Checked)
        {
            #region Sparkle

            if (sparkleType == SparkleType.Normal)
            {

            foreach (var p in keyDebrisParticles.Cast<KeyDebrisParticle>())
            {
                var aspect = options.renderAspectRatio;
                var left = layout.keys[p.key].left;
                var right = layout.keys[p.key].right;
                var pos = new Vector2d((left + right) / 2, keyboardHeight);

                double cos = Math.Cos(p.rotation);
                double sin = Math.Sin(p.rotation);
                var offset = p.pos;
                offset.Y *= aspect;
                offset *= keyboardHeight / 0.15;
                pos += offset;
                var width = layout.whiteKeyWidth;
                IO.RenderShape(
                    pos + new Vector2d(cos, sin * aspect) * width * p.size,
                   pos + new Vector2d(-sin, cos * aspect) * width * p.size,
                  pos - new Vector2d(cos, sin * aspect) * width * p.size,
                 pos - new Vector2d(-sin, cos * aspect) * width * p.size,
                whiteShade,
                prtKeyDebris
                );
            }

            foreach (var parr in keySparkParticles)
            {
                foreach (var p in parr.Cast<KeySparkParticle>())
                {
                    var brightness = 1;

                    var key = p.key;
                    var left = layout.keys[key].left;
                    var right = layout.keys[key].right;
                    var width = right - left;
                    left += width * 0.4;
                    right -= width * 0.4;
                    var pos = new Vector2d(left + (right - left) * p.pos.X, keyboardHeight + (right - left) * p.pos.Y * options.renderAspectRatio);
                    var size = width;
                    var sizey = size * options.renderAspectRatio;
                    var blendCol = new Color4(1, 1, 1, (float)(p.brightness * brightness));

                    blendCol = keyColors[p.key * 2];
                    blendCol = Util.BlendColors(blendCol, new Color4(1, 1, 1, (float)uiSparkleLightness.Value));

                    var ang = p.rotation / 5 + Math.PI * 0.75;

                    var vec = new Vector2d(Math.Cos(ang), Math.Sin(ang)) * width * p.size * 1.4;
                    var vec2 = new Vector2d(vec.Y, -vec.X);
                    vec.Y *= options.renderAspectRatio;
                    vec2.Y *= options.renderAspectRatio;

                    if (p.flipped)
                    {
                        IO.RenderShape(pos + vec2, pos + vec, pos - vec2, pos - vec, blendCol, prtKeySpark);
                    }
                    else
                    {
                        IO.RenderShape(pos + vec, pos + vec2, pos - vec, pos - vec2, blendCol, prtKeySpark);
                    }
                }
            }
            }
            else if (sparkleType == SparkleType.Rounded)
            {
                foreach (var p in keyDebrisParticles.Cast<KeyDebrisParticle>())
            {
                var aspect = options.renderAspectRatio;
                var left = layout.keys[p.key].left;
                var right = layout.keys[p.key].right;
                var pos = new Vector2d((left + right) / 2, keyboardHeight);

                double cos = Math.Cos(p.rotation);
                double sin = Math.Sin(p.rotation);
                var offset = p.pos;
                offset.Y *= aspect;
                offset *= keyboardHeight / 0.15;
                pos += offset;
                var width = layout.whiteKeyWidth;
                IO.RenderShape(
                    pos + new Vector2d(cos, sin * aspect) * width * p.size,
                   pos + new Vector2d(-sin, cos * aspect) * width * p.size,
                  pos - new Vector2d(cos, sin * aspect) * width * p.size,
                 pos - new Vector2d(-sin, cos * aspect) * width * p.size,
                whiteShade,
                prtKeyDebris
                );
            }

            foreach (var parr in keySparkParticles)
            {
                foreach (var p in parr.Cast<KeySparkParticle>())
                {
                    var brightness = 1;

                    var key = p.key;
                    var left = layout.keys[key].left;
                    var right = layout.keys[key].right;
                    var width = right - left;
                    left += width * 0.2;
                    right -= width * 0.2;
                    var pos = new Vector2d(left + (right - left) * p.pos.X, keyboardHeight + (right - left) * p.pos.Y * options.renderAspectRatio * 0.2f);
                    var size = width;
                    var sizey = size * options.renderAspectRatio;
                    var blendCol = new Color4(1, 1, 1, (float)(p.brightness * brightness));

                    blendCol = keyColors[p.key * 2];
                    blendCol = Util.BlendColors(blendCol, new Color4(1, 1, 1, (float)uiSparkleLightness.Value));

                    var ang = p.rotation / 5 + Math.PI * 0.75;

                    var vec = new Vector2d(Math.Cos(ang), Math.Sin(ang)) * width * p.size * 1.5;
                    var vec2 = new Vector2d(vec.Y, -vec.X);
                    vec.Y *= options.renderAspectRatio;
                    vec2.Y *= options.renderAspectRatio;

                    if (p.flipped)
                    {
                        IO.RenderShape(pos + vec2, pos + vec, pos - vec2, pos - vec, blendCol, prtKeySparkRnd);
                    }
                    else
                    {
                        IO.RenderShape(pos + vec, pos + vec2, pos - vec, pos - vec2, blendCol, prtKeySparkRnd);
                    }
                }
            }
            }
            else if (sparkleType == SparkleType.Flame)
            {
                foreach (var p in keyDebrisParticles.Cast<KeyDebrisParticle>())
            {
                var aspect = options.renderAspectRatio;
                var left = layout.keys[p.key].left;
                var right = layout.keys[p.key].right;
                var pos = new Vector2d((left + right) / 2, keyboardHeight);

                double cos = Math.Cos(p.rotation);
                double sin = Math.Sin(p.rotation);
                var offset = p.pos;
                offset.Y *= aspect;
                offset *= keyboardHeight / 0.15;
                pos += offset;
                var width = layout.whiteKeyWidth;
                IO.RenderShape(
                    pos + new Vector2d(cos, sin * aspect) * width * p.size,
                   pos + new Vector2d(-sin, cos * aspect) * width * p.size,
                  pos - new Vector2d(cos, sin * aspect) * width * p.size,
                 pos - new Vector2d(-sin, cos * aspect) * width * p.size,
                whiteShade,
                prtKeyDebris
                );
            }

            foreach (var parr in keySparkParticles)
            {
                foreach (var p in parr.Cast<KeySparkParticle>())
                {
                    var brightness = 1;

                    var key = p.key;
                    var left = layout.keys[key].left;
                    var right = layout.keys[key].right;
                    var width = right - left;
                    left += width * 0.7;
                    right -= width * 0.7;
                    var pos = new Vector2d(left + (right - left) * p.pos.X, keyboardHeight + (right - left) * p.pos.Y * options.renderAspectRatio);
                    var size = width;
                    var sizey = size * options.renderAspectRatio;
                    var blendCol = new Color4(1, 1, 1, (float)(p.brightness * brightness));

                    blendCol = keyColors[p.key * 2];
                    blendCol = Util.BlendColors(blendCol, new Color4(1, 1, 1, (float)uiSparkleLightness.Value));

                    var ang = p.rotation / 5 + Math.PI * 0.75;

                    var vec = new Vector2d(Math.Cos(ang), Math.Sin(ang)) * width * p.size * 1.6;
                    var vec2 = new Vector2d(vec.Y, -vec.X);
                    vec.Y *= options.renderAspectRatio * 1.7;
                    vec2.Y *= options.renderAspectRatio * 1.7;

                    if (p.flipped)
                    {
                        IO.RenderShape(pos + vec2, pos + vec, pos - vec2, pos - vec, blendCol, prtFlame);
                    }
                    else
                    {
                        IO.RenderShape(pos + vec, pos + vec2, pos - vec, pos - vec2, blendCol, prtFlame);
                    }
                }
            }
            }

            #endregion
        }
//---------Synthesia 9 KB---------

        if (uiRenderLayerKeyboard.Checked)
        {
            #region Render Keyboard

            

            var keyTop = keyboardHeight * (1 - barHeight);

            if (keyboardStyle == KeyboardStyle.S9)
            {
                #region Synthesia 9

                for (int i = firstKey; i < lastKey; i++)
                {
                    if (!layout.blackKey[i])
                    {

                        float KBWhiteVal = Convert.ToSingle(uiKeyboardWhiteness.Value) + 0.2f;

                        float CreamG = 0.98f * KBWhiteVal;
                        float CreamB = 0.9f * KBWhiteVal;

                        Color4 leftCol = Util.BlendColors(new Color4(1, (float)CreamG, (float)CreamB, 1), keyColors[i * 2]);
                        Color4 rightCol =Util.BlendColors(new Color4(1, (float)CreamG, (float)CreamB, 1), keyColors[i * 2 + 1]);

                        //Color4 leftCol = Util.BlendColors(new Color4(255, CreamG, CreamB, 255), keyColors[i * 2]);
                        //Color4 rightCol = Util.BlendColors(new Color4(255, CreamG, CreamB, 255), keyColors[i * 2 + 1]);

                        var part = layout.keyNumber[i] % 7;

                        var s9KeyWhitePressed = this.s9KeyWhitePressed;
                        var s9KeyWhiteWholePressed = this.s9KeyWhiteWholePressed;
                        var s9KeyWhite = this.s9KeyWhite;
                        var s9KeyWhiteWhole = this.s9KeyWhiteWhole;

                        if (keyboardS9Alt)
                        {
                            s9KeyWhitePressed = this.s9aKeyWhitePressed;
                            s9KeyWhiteWholePressed = this.s9aKeyWhiteWhole;
                            s9KeyWhite = this.s9aKeyWhite;
                            s9KeyWhiteWhole = this.s9aKeyWhiteWhole;
                        }

                        var split = 0.5;
                        if (part == 3) split = 0.3;
                        if (part == 6) split = 0.7;

                        var uvLeft = part * (1 / 7.0);
                        var uvRight = (part + 1) * (1 / 7.0);
                        var uvMiddle = (part + split) * (1 / 7.0);

                        var left = layout.keys[i].left;
                        var right = layout.keys[i].right;
                        var middle = left + (right - left) * split;

                        if (i == lastKey - 1)
                        {
                            if (keyPressed[i])
                            {
                                IO.RenderQuad(left, keyTop, middle, 0, leftCol, leftCol, rightCol, rightCol, s9KeyWhitePressed, uvLeft, 0, uvMiddle, 1);
                                IO.RenderQuad(middle, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, s9KeyWhiteWholePressed, split, 0, 1, 1);
                            }
                            else
                            {
                                IO.RenderQuad(left, keyTop, middle, 0, leftCol, leftCol, rightCol, rightCol, s9KeyWhite, uvLeft, 0, uvMiddle, 1);
                                IO.RenderQuad(middle, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, s9KeyWhiteWhole, split, 0, 1, 1);
                            }
                        }
                        else if (i == firstKey)
                        {
                            if (keyPressed[i])
                            {
                                IO.RenderQuad(left, keyTop, middle, 0, leftCol, leftCol, rightCol, rightCol, s9KeyWhiteWholePressed, 0, 0, split, 1);
                                IO.RenderQuad(middle, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, s9KeyWhitePressed, uvMiddle, 0, uvRight, 1);
                            }
                            else
                            {
                                IO.RenderQuad(left, keyTop, middle, 0, leftCol, leftCol, rightCol, rightCol, s9KeyWhiteWhole, 0, 0, split, 1);
                                IO.RenderQuad(middle, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, s9KeyWhite, uvMiddle, 0, uvRight, 1);
                            }
                        }
                        else
                        {
                            if (keyPressed[i])
                            {
                                IO.RenderQuad(left, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, s9KeyWhitePressed, uvLeft, 0, uvRight, 1);
                            }
                            else
                            {
                                IO.RenderQuad(left, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, s9KeyWhite, uvLeft, 0, uvRight, 1);
                            }
                        }

                        #region Shadows
                        if (i != 0 && i != firstKey)
                        {
                            Action renderLarge = () =>
                            {
                                var shadowRight = left + keyTop * s9ShadowLarge.aspectRatio / options.renderAspectRatio;
                                IO.RenderQuad(left, keyTop, shadowRight, 0, whiteShade, s9ShadowLarge, 0, 0, 1, 1);
                            };

                            Action renderUnpressed = () =>
                            {
                                var shadowRight = layout.keys[i - 1].right + keyTop * s9ShadowUnpressed.aspectRatio / options.renderAspectRatio;
                                IO.RenderQuad(layout.keys[i - 1].right, keyTop, shadowRight, 0, whiteShade, s9ShadowUnpressed, 0, 0, 1, 1);
                            };

                            Action renderPressed = () =>
                            {
                                var shadowRight = layout.keys[i - 1].right + keyTop * s9ShadowPressed.aspectRatio / options.renderAspectRatio;
                                IO.RenderQuad(layout.keys[i - 1].right, keyTop, shadowRight, 0, whiteShade, s9ShadowPressed, 0, 0, 1, 1);
                            };

                            if (keyPressed[i])
                            {
                                if (layout.blackKey[i - 1])
                                {
                                    if (!keyPressed[i - 2])
                                    {
                                        renderLarge();
                                    }

                                    if (keyPressed[i - 1])
                                    {
                                        renderPressed();
                                    }
                                    else
                                    {
                                        renderUnpressed();
                                    }
                                }
                                else
                                {
                                    if (!keyPressed[i - 1])
                                    {
                                        renderLarge();
                                    }
                                }
                            }
                            else
                            {
                                if (layout.blackKey[i - 1])
                                {
                                    if (!keyPressed[i - 1])
                                    {
                                        renderPressed();
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }

                IO.RenderQuad(0, keyboardHeight, 1, keyTop, whiteShade, s9Bar);

                for (int i = firstKey; i < lastKey; i++)
                {
                    if (layout.blackKey[i])
                    {
                        Color4 leftCol = Util.BlendColors(new Color4(255, 255, 255, 255), keyColors[i * 2]);
                        Color4 rightCol = Util.BlendColors(new Color4(255, 255, 255, 255), keyColors[i * 2 + 1]);

                        var part = layout.keyNumber[i] % 5;

                        var bKeyBottom = keyboardHeight / 2.9;
                        var bKeyTopPressed = keyTop + keyboardHeight * 0.006;
                        var bKeyTopUnpressed = keyTop + keyboardHeight * 0.015;

                        var uvLeft = part * (1 / 5.0);
                        var uvRight = (part + 1) * (1 / 5.0);

                        double left = layout.keys[i].left;
                        double right = layout.keys[i].right;

                        if (keyPressed[i])
                        {
                            IO.RenderQuad(left, bKeyTopPressed, right, bKeyBottom, leftCol, leftCol, rightCol, rightCol, s9KeyBlackPressed, uvLeft, 0, uvRight, 1);
                        }
                        else
                        {
                            IO.RenderQuad(left, bKeyTopUnpressed, right, bKeyBottom, leftCol, leftCol, rightCol, rightCol, s9KeyBlack, uvLeft, 0, uvRight, 1);
                        }
                    }
                }

                #endregion
            }
//---------Synthesia 8 KB---------

            else if (keyboardStyle == KeyboardStyle.S8)
            {
                #region  Synthesia 8

                for (int i = firstKey; i < lastKey; i++)
                {
                    if (!layout.blackKey[i])
                    {
                        Color4 leftCol = Util.BlendColors(new Color4(255, 255, 255, 255), keyColors[i * 2]);
                        Color4 rightCol = Util.BlendColors(new Color4(255, 255, 255, 255), keyColors[i * 2 + 1]);

                        var part = layout.keyNumber[i] % 7;

                        var split = 0.5;
                        if (part == 3) split = 0.3;
                        if (part == 6) split = 0.7;

                        var uvLeft = part * (1 / 7.0);
                        var uvRight = (part + 1) * (1 / 7.0);
                        var uvMiddle = (part + split) * (1 / 7.0);

                        var left = layout.keys[i].left;
                        var right = layout.keys[i].right;
                        var middle = left + (right - left) * split;

                        if (i == lastKey - 1)
                        {
                            if (keyPressed[i])
                            {
                                IO.RenderQuad(left, keyTop, middle, 0, leftCol, leftCol, rightCol, rightCol, s8KeyWhitePressed, uvLeft, 0, uvMiddle, 1);
                                IO.RenderQuad(middle, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, s8KeyWhiteWholePressed, split, 0, 1, 1);
                            }
                            else
                            {
                                IO.RenderQuad(left, keyTop, middle, 0, leftCol, leftCol, rightCol, rightCol, s8KeyWhite, uvLeft, 0, uvMiddle, 1);
                                IO.RenderQuad(middle, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, s8KeyWhiteWhole, split, 0, 1, 1);
                            }
                        }
                        else if (i == firstKey)
                        {
                            if (keyPressed[i])
                            {
                                IO.RenderQuad(left, keyTop, middle, 0, leftCol, leftCol, rightCol, rightCol, s8KeyWhiteWholePressed, 0, 0, split, 1);
                                IO.RenderQuad(middle, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, s8KeyWhitePressed, uvMiddle, 0, uvRight, 1);
                            }
                            else
                            {
                                IO.RenderQuad(left, keyTop, middle, 0, leftCol, leftCol, rightCol, rightCol, s8KeyWhiteWhole, 0, 0, split, 1);
                                IO.RenderQuad(middle, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, s8KeyWhite, uvMiddle, 0, uvRight, 1);
                            }
                        }
                        else
                        {
                            if (keyPressed[i])
                            {
                                IO.RenderQuad(left, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, s8KeyWhitePressed, uvLeft, 0, uvRight, 1);
                            }
                            else
                            {
                                IO.RenderQuad(left, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, s8KeyWhite, uvLeft, 0, uvRight, 1);
                            }
                        }
                    }
                }

                IO.RenderQuad(0, keyboardHeight, 1, keyTop, whiteShade, s8Bar);

                for (int i = firstKey; i < lastKey; i++)
                {
                    if (layout.blackKey[i])
                    {
                        Color4 leftCol = Util.BlendColors(new Color4(255, 255, 255, 255), keyColors[i * 2]);
                        Color4 rightCol = Util.BlendColors(new Color4(255, 255, 255, 255), keyColors[i * 2 + 1]);

                        var part = layout.keyNumber[i] % 5;

                        var bKeyBottom = keyboardHeight / 2.8;
                        var bKeyTop = keyTop + keyboardHeight * 0.023;

                        var uvLeft = part * (1 / 5.0);
                        var uvRight = (part + 1) * (1 / 5.0);

                        double left = layout.keys[i].left;
                        double right = layout.keys[i].right;

                        if (keyPressed[i])
                        {
                            IO.RenderQuad(left, bKeyTop, right, bKeyBottom, leftCol, leftCol, rightCol, rightCol, s8KeyBlackPressed);
                        }
                        else
                        {
                            IO.RenderQuad(left, bKeyTop, right, bKeyBottom, leftCol, leftCol, rightCol, rightCol, s8KeyBlack);
                        }
                    }
                }

                #endregion
            }
//---------Synthesia 7 KB---------

            else if (keyboardStyle == KeyboardStyle.S7)
            {
                #region  Synthesia 7

                for (int i = firstKey; i < lastKey; i++)
                {
                    if (!layout.blackKey[i])
                    {
                        Color4 leftCol = Util.BlendColors(new Color4(255, 255, 255, 255), keyColors[i * 2]);
                        Color4 rightCol = Util.BlendColors(new Color4(255, 255, 255, 255), keyColors[i * 2 + 1]);

                        var part = layout.keyNumber[i] % 7;

                        var split = 0.5;
                        if (part == 3) split = 0.3;
                        if (part == 6) split = 0.7;

                        var uvLeft = part * (1 / 7.0);
                        var uvRight = (part + 1) * (1 / 7.0);
                        var uvMiddle = (part + split) * (1 / 7.0);

                        var left = layout.keys[i].left;
                        var right = layout.keys[i].right;
                        var middle = left + (right - left) * split;

                        if (i == lastKey - 1)
                        {
                            if (keyPressed[i])
                            {
                                IO.RenderQuad(left, keyTop, middle, 0, leftCol, leftCol, rightCol, rightCol, s7KeyWhitePressed, uvLeft, 0, uvMiddle, 1);
                                IO.RenderQuad(middle, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, s7KeyWhiteWholePressed, split, 0, 1, 1);
                            }
                            else
                            {
                                IO.RenderQuad(left, keyTop, middle, 0, leftCol, leftCol, rightCol, rightCol, s7KeyWhite, uvLeft, 0, uvMiddle, 1);
                                IO.RenderQuad(middle, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, s7KeyWhiteWhole, split, 0, 1, 1);
                            }
                        }
                        else if (i == firstKey)
                        {
                            if (keyPressed[i])
                            {
                                IO.RenderQuad(left, keyTop, middle, 0, leftCol, leftCol, rightCol, rightCol, s7KeyWhiteWholePressed, 0, 0, split, 1);
                                IO.RenderQuad(middle, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, s7KeyWhitePressed, uvMiddle, 0, uvRight, 1);
                            }
                            else
                            {
                                IO.RenderQuad(left, keyTop, middle, 0, leftCol, leftCol, rightCol, rightCol, s7KeyWhiteWhole, 0, 0, split, 1);
                                IO.RenderQuad(middle, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, s7KeyWhite, uvMiddle, 0, uvRight, 1);
                            }
                        }
                        else
                        {
                            if (keyPressed[i])
                            {
                                IO.RenderQuad(left, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, s7KeyWhitePressed, uvLeft, 0, uvRight, 1);
                            }
                            else
                            {
                                IO.RenderQuad(left, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, s7KeyWhite, uvLeft, 0, uvRight, 1);
                            }
                        }
                    }
                }

                IO.RenderQuad(0, keyboardHeight, 1, keyTop, whiteShade, s7Bar);

                for (int i = firstKey; i < lastKey; i++)
                {
                    if (layout.blackKey[i])
                    {
                        Color4 leftCol = Util.BlendColors(new Color4(255, 255, 255, 255), keyColors[i * 2]);
                        Color4 rightCol = Util.BlendColors(new Color4(255, 255, 255, 255), keyColors[i * 2 + 1]);

                        var part = layout.keyNumber[i] % 5;

                        var bKeyBottom = keyboardHeight / 2.755;

                        var uvLeft = part * (1 / 5.0);
                        var uvRight = (part + 1) * (1 / 5.0);

                        double left = layout.keys[i].left;
                        double right = layout.keys[i].right;

                        if (keyPressed[i])
                        {
                            IO.RenderQuad(left, keyTop, right, bKeyBottom, leftCol, leftCol, rightCol, rightCol, s8KeyBlackPressed);
                        }
                        else
                        {
                            IO.RenderQuad(left, keyTop, right, bKeyBottom, leftCol, leftCol, rightCol, rightCol, s8KeyBlack);
                        }
                    }
                }
                #endregion
            }
            #endregion
//---------Glow KB---------
        else if (keyboardStyle == KeyboardStyle.Glow)
            {
                #region Glowing

                for (int i = firstKey; i < lastKey; i++)
                {
                    if (!layout.blackKey[i])
                    {
                        IO.SelectTextureShader(TextureShaders.Hybrid);

                        Color4 leftCol = Util.BlendColors(new Color4(128, 128, 128, 255), keyColors[i * 2]);
                        Color4 rightCol = Util.BlendColors(new Color4(128, 128, 128, 255), keyColors[i * 2 + 1]);

                        var part = layout.keyNumber[i] % 7;

                        var glowKeyWhitePressed = this.glowKeyWhitePressed;
                        var glowKeyWhiteWholePressed = this.glowKeyWhiteWholePressed;
                        var glowKeyWhite = this.glowKeyWhite;
                        var glowKeyWhiteWhole = this.glowKeyWhiteWhole;

                        if (keyboardGlowAlt)
                        {
                            glowKeyWhitePressed = this.glowaKeyWhitePressed;
                            glowKeyWhiteWholePressed = this.glowaKeyWhiteWholePressed;
                            glowKeyWhite = this.glowaKeyWhite;
                            glowKeyWhiteWhole = this.glowaKeyWhiteWhole;
                        }

                        var split = 0.5;
                        if (part == 3) split = 0.3;
                        if (part == 6) split = 0.7;

                        var uvLeft = part * (1 / 7.0);
                        var uvRight = (part + 1) * (1 / 7.0);
                        var uvMiddle = (part + split) * (1 / 7.0);

                        var left = layout.keys[i].left;
                        var right = layout.keys[i].right;
                        var middle = left + (right - left) * split;

                        if (i == lastKey - 1)
                        {
                            if (keyPressed[i])
                            {
                                IO.RenderQuad(left, keyTop, middle, 0, leftCol, leftCol, rightCol, rightCol, glowKeyWhitePressed, uvLeft, 0, uvMiddle, 1);
                                IO.RenderQuad(middle, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, glowKeyWhiteWholePressed, split, 0, 1, 1);
                            }
                            else
                            {
                                IO.RenderQuad(left, keyTop, middle, 0, leftCol, leftCol, rightCol, rightCol, glowKeyWhite, uvLeft, 0, uvMiddle, 1);
                                IO.RenderQuad(middle, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, glowKeyWhiteWhole, split, 0, 1, 1);
                            }
                        }
                        else if (i == firstKey)
                        {
                            if (keyPressed[i])
                            {
                                IO.RenderQuad(left, keyTop, middle, 0, leftCol, leftCol, rightCol, rightCol, glowKeyWhiteWholePressed, 0, 0, split, 1);
                                IO.RenderQuad(middle, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, glowKeyWhitePressed, uvMiddle, 0, uvRight, 1);
                            }
                            else
                            {
                                IO.RenderQuad(left, keyTop, middle, 0, leftCol, leftCol, rightCol, rightCol, glowKeyWhiteWhole, 0, 0, split, 1);
                                IO.RenderQuad(middle, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, glowKeyWhite, uvMiddle, 0, uvRight, 1);
                            }
                        }
                        else
                        {
                            if (keyPressed[i])
                            {
                                IO.RenderQuad(left, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, glowKeyWhitePressed, uvLeft, 0, uvRight, 1);
                            }
                            else
                            {
                                IO.RenderQuad(left, keyTop, right, 0, leftCol, leftCol, rightCol, rightCol, glowKeyWhite, uvLeft, 0, uvRight, 1);
                            }
                        }
                        
                    }
                }

                IO.RenderQuad(0, keyboardHeight, 1, keyTop, whiteShade, glowBar);

                for (int i = firstKey; i < lastKey; i++)
                {
                    if (layout.blackKey[i])
                    {
                        IO.SelectTextureShader(TextureShaders.Hybrid);

                        Color4 leftCol = Util.BlendColors(new Color4(128, 128, 128, 255), keyColors[i * 2]);
                        Color4 rightCol = Util.BlendColors(new Color4(128, 128, 128, 255), keyColors[i * 2 + 1]);

                        var part = layout.keyNumber[i] % 5;

                        var bKeyBottom = keyboardHeight / 2.9;
                        var bKeyTopPressed = keyTop + keyboardHeight * 0.006;
                        var bKeyTopUnpressed = keyTop + keyboardHeight * 0.015;

                        var uvLeft = part * (1 / 5.0);
                        var uvRight = (part + 1) * (1 / 5.0);

                        double left = layout.keys[i].left;
                        double right = layout.keys[i].right;

                        if (keyPressed[i])
                        {
                            IO.RenderQuad(left, bKeyTopPressed, right, bKeyBottom, leftCol, leftCol, rightCol, rightCol, glowKeyBlackPressed, uvLeft, 0, uvRight, 1);
                        }
                        else
                        {
                            IO.RenderQuad(left, bKeyTopUnpressed, right, bKeyBottom, leftCol, leftCol, rightCol, rightCol, glowKeyBlack, uvLeft, 0, uvRight, 1);
                        }
                    }
                }

                #endregion
            }
//---------Overlays---------

            #region Overlays

            if (uiOverlay.Checked && keyboardStyle == KeyboardStyle.S9)
            {
                for (int k = firstKey; k < lastKey; k++)
                {
                    if (k % 12 == 0)
                    {
                        var i = k / 12;

                        if (uiOverlayn1.Checked) i--;

                        var left = layout.keys[k].left;
                        var right = layout.keys[k].right;

                        var size = 1.7 * uiOverlayScale.Value * uiKeyboardScale.Value;
                        var height = 1.0 * size;
                        var spacing = 0.6;
                        var offset = 0.0;
                        if (i >= 10)
                        {
                            height = 0.75 * size;
                            spacing = 0.55;
                            offset = -0.02;
                        }
                        if (i == -1)
                        {
                            height = 0.8 * size;
                            spacing = 0.55;
                            offset = -0.02;
                        }
                        height *= (right - left);

                        string txt = "C" + i;

                        var middle = (right + left) / 2 + (right - left) * offset;

                        List<double> widths = new List<double>();
                        foreach (var c in txt)
                        {
                            if (c == '-') widths.Add(IO.GetTextWidth(cFont, height, c.ToString()) * spacing * 0.7);
                            else widths.Add(IO.GetTextWidth(cFont, height, c.ToString()) * spacing);
                        }
                        var width = widths.Sum();

                        var start = middle - width / 2;

                        int ci = 0;
                        Color4 col = new Color4(0, 0, 0, 0.36f);
                        if (k == 5 * 12) col = new Color4(0, 0, 0, 0.64f);
                        foreach (var c in txt)
                        {
                            IO.RenderText(start, (right - left) * options.renderAspectRatio * 0.25, height, col, cFont, c.ToString());
                            start += widths[ci];
                            ci++;
                        }
                    }
                }
            }
            if (uiCredit.Checked)
            {
                int k = firstKey;
                var left = layout.keys[k].left;
                var right = layout.keys[k].right;
                var offset = 0.0;
                var middle = (right + left) / 2 + (right - left) * offset;
                List<double> widths = new List<double>();
                var width = widths.Sum();
                var size = 0.05;
                var height = 1.0 * size;

                var start = middle - width / 5;
                Color4 col = new Color4(0, 0, 0, 0.36f);
                IO.RenderText(start, (right - left) * options.renderAspectRatio * 7, height, col, creditFont, "Zenith");
            }

            #endregion
        }
//---------Flare---------

        for (int i = 0; i < particles.Length; i++)
        {
            if (uiRenderParticleLayers[i].Checked)
            {
                particles[i].RenderOverKeys(layout, keyboardHeight, options);
            }
        }

        if (uiRenderLayerFlares.Checked)
        {
            #region Note Flares

            if (uiRenderKeyFlare.Checked)
            {
                IO.SetBlendFunc(BlendFunc.Add);
                for (int i = firstKey; i < lastKey; i++)
                {
                    if (keyPressed[i])
                    {
                        keyFlareStrength[i] += 1 / uiKeyFlareAttack.Value / options.renderFPS;
                        if (keyFlareStrength[i] > 1) keyFlareStrength[i] = 1;
                    }
                    else
                    {
                        keyFlareStrength[i] -= 1 / uiKeyFlareRelease.Value / options.renderFPS;
                        if (keyFlareStrength[i] < 0) keyFlareStrength[i] = 0;
                    }

                    if (keyFlareStrength[i] > 0)
                    {
                        var extraW = 0.08;
                        var left = layout.keys[i].left - layout.whiteKeyWidth * extraW;
                        var right = layout.keys[i].right + layout.whiteKeyWidth * extraW;
                        var height = layout.whiteKeyWidth * uiKeyFlareHeight.Value;
                        var offset = 0.4;
                        var yOffset = height * offset;

                        var blendCol = keyColors[i * 2];
                        blendCol = Util.BlendColors(blendCol, new Color4(1, 1, 1, (float)uiFlareLightness.Value));

                        var col = blendCol;
                        col.A *= (float)(keyFlareStrength[i] * uiKeyFlareStrength.Value);
                        IO.RenderQuad(left, keyboardHeight + height - yOffset, right, keyboardHeight, col, noteFlare, 0, 1, 1, offset);
                        
                        col = blendCol;
                        col.A *= (float)(keyFlareStrength[i] * uiKeyFlareStrengthDown.Value);
                        IO.RenderQuad(left, keyboardHeight, right, keyboardHeight - yOffset, col, noteFlare, 0, offset, 1, 0);
                    }
                }
                IO.SetBlendFunc(BlendFunc.Mix);
            }

            #endregion
        }
//---------Sparkle Glow---------

        if (uiRenderLayerSparkleGlow.Checked)
        {
            #region Sparkle Glow

            foreach (var parr in keyHazeParticles)
            {
                foreach (var p in parr.Cast<KeyHazeParticle>())
                {
                    var brightness = 0.15 * uiSparkleGlowLightness.Value;

                    var key = p.key;
                    var left = layout.keys[key].left;
                    var right = layout.keys[key].right;
                    var width = right - left;
                    left += width * 0.4;
                    right -= width * 0.4;
                    var pos = new Vector2d(left + (right - left) * p.pos.X, keyboardHeight + (right - left) * p.pos.Y * options.renderAspectRatio);
                    pos.Y += keyboardHeight * 0.02;
                    var size = width * 1.8;
                    var sizey = size * options.renderAspectRatio;
                    var blendCol = new Color4(1, 1, 1, (float)(p.brightness * brightness));
                    IO.RenderQuad(pos.X - size, pos.Y + sizey, pos.X + size, pos.Y - sizey, blendCol, prtKeyHaze);
                }
            }

            #endregion
        }
//---------Kill Particles---------

        #region Kill Particles

        foreach (var arr in fullParticlesArray)
        {
            var node = arr.First;
            while (node != null)
            {
                var p = node.Value;
                p.Step(1.0 / options.renderFPS);

                var _node = node;
                node = node.Next;
                if (p.life <= 0)
                {
                    arr.Remove(_node);
                }
            }
        }
        #endregion
        }

    public void RenderInit(RenderOptions options)
    {
        keyFlareStrength = new double[257];
        for (int i = 0; i < 256; i++)
        {
            keyHitNotes[i] = null;
            keyHitNotesPrevious[i] = null;
        }
    }

    public void RenderDispose()
    {
        foreach (var arr in fullParticlesArray) arr.Clear();
        for (int i = 0; i < 256; i++)
        {
            keyHitNotes[i] = null;
            keyHitNotesPrevious[i] = null;
        }
        foreach (var p in particles)
            p.Reset();
    }
    }
//---------Particle System---------
    class ParticleSystem
    {
    LinkedList<KeyParticle> particles = new LinkedList<KeyParticle>();

    Texture prtKeySpark = IO.LoadTexture("Particles/Synthesia/keyDebrisCentered.png");
    Texture prtKeyBlob = IO.LoadTexture("Particles/Synthesia/keyHaze.png");
    Texture prtKeySquare = IO.LoadTexture("Particles/Synthesia/square.png");
    Texture prtKeyCircle = IO.LoadTexture("Particles/Synthesia/circle.png");

    Random r = new Random();

    UINumberSlider uiAccelVert = new UINumberSlider("Vertical     ", -0.03, -0.2, 0.2, -10, 10, 2, 0.01);
    UINumberSlider uiAccelHor = new UINumberSlider("Horizontal", 0, -0.2, 0.2, -10, 10, 2, 0.01);
    UINumberSlider uiAirDrag = new UINumberSlider("", 0.1, 0, 0.2, 0, 1, 2, 0.01);
    UINumberSlider uiParticleLife = new UINumberSlider("Average Life", 2, 0, 20, 0, 200, 2, 0.1);
    UINumberSlider uiParticleLifeRandom = new UINumberSlider("Life Randomness", 1, 0, 20, 0, 200, 2, 0.1);
    UINumberSlider uiParticleLifeFade = new UINumberSlider("Fade", 1, 0, 20, 0, 200, 2, 0.1);
    UINumberSlider uiParticleLifeSize = new UINumberSlider("Shrink", 1, 0, 20, 0, 200, 2, 0.1);
    UINumberSlider uiLightness = new UINumberSlider("Lightness", 0.2, 0, 1, 0, 1, 2, 0.01);
    UINumberSlider uiOpacity = new UINumberSlider("Opacity", 1, 0, 1, 0, 1, 2, 0.01);
    UINumberSlider uiVelocity = new UINumberSlider("Average Vel.       ", 2, 0, 10, 0, 1000, 2, 0.1);
    UINumberSlider uiVelocityRand = new UINumberSlider("Vel. Randomness", 0.5, 0, 2, 0, 100, 2, 0.1);
    UINumberSlider uiVelAngle = new UINumberSlider("Average Launch Angle       ", 0.5, 0, 2, 0, 2, 2, 0.1);
    UINumberSlider uiVelAngleRand = new UINumberSlider("Launch Angle Randomness", 0.25, 0, 1, 0, 1, 2, 0.1);
    UICheckbox uiShader = new UICheckbox("Glow (Add) shader", true);
    UINumberSlider uiSpawnrate = new UINumberSlider("Spawn Rate", 0.5, 0, 0.99, 0, 0.99, 2, 0.01);
    UIDropdown uiParticleTex = new UIDropdown("Texture", 0, new[] { "Blob", "Spark", "Square", "Circle" });
    UINumberSlider uiRotSpeed = new UINumberSlider("", 0.2, 0, 2, 0, 20, 2, 0.1);
    UINumberSlider uiSize = new UINumberSlider("Size", 0.2, 0, 3, 0, 20, 2, 0.01);
    UINumberSlider uiSizeRand = new UINumberSlider("Size Randomness", 0.1, 0, 2, 0, 20, 2, 0.01);
    UICheckbox uiCollide = new UICheckbox("Collide with ground", true);
    UINumberSlider uiDampen = new UINumberSlider("Dampen", 0.3, 0, 2, 0, 20, 2, 0.1);
    UINumberSlider uiCollideSize = new UINumberSlider("Collision Radius", 0.8, 0.1, 1, 0.1, 1, 2, 0.01);
    UINumberSlider uiTurbulance = new UINumberSlider("Strength", 0.0, 0, 2, 0, 10, 2, 0.01);
    UINumberSlider uiTurbulanceScale = new UINumberSlider("Scale", 0.3, 0.1, 5, 0.1, 5, 2, 0.01, true);
    UINumberSlider uiMutationSpeed = new UINumberSlider("Mutation Speed", 1, 0, 5, 0, 20, 2, 0.1);
    UINumberSlider uiMaxParticles = new UINumberSlider("Max Particles", 50000, 1, 1000000, 1, 10000000, 0, true);
    UINumberSlider uiSimulationSpeed = new UINumberSlider("Simulation Speed Factor", 1, 0.01, 3, 0.01, 20, 2, 0.1, true);
    UINumber uiSimulationSubsteps = new UINumber("Simulation Substeps", 1, 1, 10, 0, 1);
    UIDropdown uiParticleSpawnArea = new UIDropdown("Spawn Location", 0, new[] { "None", "Pressed keys", "Beneath notes", "Top of notes" }) { Padding = 0 };
    bool spawnNone { get { return uiParticleSpawnArea.Index == 0; } }
    bool spawnOnKeys { get { return uiParticleSpawnArea.Index == 1; } }
    bool spawnUnderNotes { get { return uiParticleSpawnArea.Index == 2; } }
    bool spawnAboveNotes { get { return uiParticleSpawnArea.Index == 3; } }
    UINumberSlider uiSpawnAreaWidth = new UINumberSlider("Spawn Area Width", 0.5, 0, 1, 0, 1, 2, 0.1) { Padding = 0 };

    UIDropdown uiRenderPosition = new UIDropdown("Render Position", 1, new[] { "Under Notes", "Under Keyboard", "Above Keyboard" }) { Padding = 0 };
    bool renderOnKeys { get { return uiRenderPosition.Index == 2; } }
    bool renderUnderKeys { get { return uiRenderPosition.Index == 1; } }
    bool renderUnderNotes { get { return uiRenderPosition.Index == 0; } }

    UIDropdown uiSpawnChildren;
    UICheckbox uiStreakMode = new UICheckbox("Streak mode", true) { Padding = 0 };
    UINumberSlider uiStreakSpacing = new UINumberSlider("Streak Pasticle Spacing", 0.2, 0.01, 3, 0.01, 100, 2, 0.1);

    double turbMutation = 0;

    ParticleSystem[] otherSystems;

    public IEnumerable<UISetting> GetSettings(ParticleSystem[] otherSystems)
    {
        this.otherSystems = otherSystems;

        List<string> spawnChildren = new List<string>() { "None" };
        for (int i = 0; i < otherSystems.Length; i++)
        {
            spawnChildren.Add("System " + (i + 1));
        }
        uiSpawnChildren = new UIDropdown("Spawn Location", 0, spawnChildren.ToArray());

        return new[] {

            new UITabs(new Dictionary<string, IEnumerable<UISetting>>() {
                {"Spawn",
                    new UISetting[] {
                        uiSpawnrate,
                        uiParticleSpawnArea,
                        new UILabel("Must be \"None\" if this is used as a child particle system", 12) { Padding = 10 },
                        uiSpawnAreaWidth,
                        new UILabel("Width from the center of the spawn area", 12) { Padding = 10 },
                    }
                },
                {"Life",
                    new UISetting[] {
                        new UILabel("Particle Life (seconds)", 20) { Padding = 0 },
                        uiParticleLife,
                        uiParticleLifeRandom,
                        new UILabel("Death behavior", 20) { Padding = 0 },
                        uiParticleLifeFade,
                        uiParticleLifeSize
                    }
                },
                {"Size",
                    new UISetting[] {
                        uiSize,
                        uiSizeRand
                    }
                },
                {"Velocity",
                    new UISetting[] {
                        new UILabel("Cardinal", 20) { Padding = 0 },
                        uiVelocity,
                        uiVelocityRand,
                        uiVelAngle,
                        uiVelAngleRand,
                        new UILabel("Rotational", 20) { Padding = 0 },
                        uiRotSpeed
                    }
                },
                {"Forces",
                    new UISetting[] {
                        new UILabel("Gravity (constant)", 20) { Padding = 0 },
                        uiAccelVert,
                        uiAccelHor,
                        new UILabel("Air Drag", 20) { Padding = 0 },
                        uiAirDrag
                    }
                },
                {"Turbulence",
                    new UISetting[] {
                        uiTurbulance,
                        uiTurbulanceScale,
                        uiMutationSpeed
                    }
                },
                {"Ground",
                    new UISetting[] {
                        uiCollide,
                        uiDampen,
                        uiCollideSize
                    }
                },
                {"Appearance",
                    new UISetting[] {
                        uiParticleTex,
                        uiRenderPosition,
                        uiLightness,
                        uiOpacity,
                        uiShader,
                    }
                },
                {"Child Particles",
                    new UISetting[] {
                        new UILabel("Child particles", 20) { Padding = 0 },
                        uiSpawnChildren,
                        uiStreakMode,
                        new UILabel("Spawns child particles with no gaps between them based on their average size and streak particle spacing", 12) { Padding = 10 },
                        uiStreakSpacing
                    }
                },
                {"Other",
                    new UISetting[] {
                        uiMaxParticles,
                        uiSimulationSpeed,
                        uiSimulationSubsteps
                    }
                },
            })
        };
    }

    KeyParticle SpawnParticle(Vector2d pos, Color4 col)
    {
        var ang = Math.PI * uiVelAngle.Value + (r.NextDouble() - 0.5) * Math.PI * 2 * uiVelAngleRand.Value;

        var vel = uiVelocity.Value + (r.NextDouble() - 0.5) * uiVelocityRand.Value * 2;

        var life = uiParticleLife.Value + (r.NextDouble() - 0.5) * uiParticleLifeRandom.Value * 2;
        var size = uiSize.Value + (r.NextDouble() - 0.5) * uiSizeRand.Value * 2;

        var particle = new KeyParticle(
            pos,
            r.NextDouble() * Math.PI * 2,
            (r.NextDouble() - 0.5) * uiRotSpeed.Value,
            new Vector2d(Math.Cos(ang) * vel, Math.Sin(ang) * vel),
            col,
            size,
            life
        );

        particles.AddLast(particle);

        while (particles.Count > uiMaxParticles.Value) particles.RemoveFirst();

        return particle;
    }


    void StepSimulation(double timeFactor, double aspect)
    {
        turbMutation += timeFactor * uiMutationSpeed.Value;
        var node = particles.First;
        while (node != null)
        {
            var p = node.Value;
            p.Push(timeFactor, new Vector2d(uiAccelHor.Value, uiAccelVert.Value));
            p.Push(timeFactor, -p.vel * uiAirDrag.Value);

            if (uiCollide.Checked)
            {
                if (p.pos.Y < 0)
                {
                    // Bounce
                    p.pos.Y *= -1;
                    p.vel.Y *= -1;
                    p.vel.Y -= uiDampen.Value / 1;
                    if (p.vel.Y < 0)
                    {
                        p.vel.Y = 0;
                        p.pos.Y = 0;
                    }

                    // Friction
                    double surfaceVel = p.rotationVel * p.size * uiCollideSize.Value;
                    double diff = p.vel.X - surfaceVel;
                    p.vel.X -= diff * 0.2;
                    p.rotationVel += diff * 0.2 / p.size / uiCollideSize.Value;
                    if (p.vel.Y == 0 && p.pos.Y == 0)
                        p.Push(timeFactor, new Vector2d(-p.vel.X * 0.1, 0));
                    else p.vel.X *= 0.9;
                }
            }

            if (uiTurbulance.Value > 0.005)
            {
                var xTurb = NoiseMaker.Noise(p.pos.X * uiTurbulanceScale.Value, p.pos.Y * uiTurbulanceScale.Value, turbMutation);
                var yTurb = NoiseMaker.Noise(p.pos.X * uiTurbulanceScale.Value, p.pos.Y * uiTurbulanceScale.Value, turbMutation + 100);

                p.Push(timeFactor, new Vector2d(xTurb, yTurb) * uiTurbulance.Value);
            }

            var prevPos = p.pos;

            p.Step(timeFactor, aspect);

            var newPos = p.pos;

            if (uiSpawnChildren.Index != 0)
            {
                var system = uiSpawnChildren.Index - 1;
                var sys = otherSystems[system];
                if (sys.spawnNone)
                {
                    if (!uiStreakMode.Checked)
                    {
                        var baseRate = sys.uiSpawnrate.Value / (1 - sys.uiSpawnrate.Value);
                        baseRate *= timeFactor;

                        var rate = baseRate;
                        rate = rate / (1 + rate);
                        while (rate > r.NextDouble())
                        {
                            sys.SpawnParticle(newPos, p.color);
                        }
                    }
                    else
                    {
                        var offset = newPos - prevPos;
                        var len = offset.Length;
                        var size = sys.uiSize.Value;
                        if (size < 0.01) size = 0.01;
                        size *= uiStreakSpacing.Value;

                        var d = p.streakExcess;
                        var dEnd = len;

                        while (d < dEnd)
                        {
                            sys.SpawnParticle(prevPos + offset * d / len, p.color);

                            d += size;
                        }

                        p.streakExcess = d - dEnd;
                    }
                }
            }

            var _node = node.Next;
            if (p.life <= 0) particles.Remove(node);
            node = _node;
        }

        if (uiSpawnChildren.Index != 0)
        {
            var system = uiSpawnChildren.Index - 1;
            var sys = otherSystems[system];
            if (sys.spawnNone)
            {
                sys.StepSimulation(timeFactor, aspect);
            }
            else
            {
                Console.WriteLine("Can't spawn particle to a system that isn't set as \"None\" for Spawn Socation");
            }
        }
    }

    public void GeneratOnKeys(Color4[] colors, int[] pressed, int firstKey, int lastKey, KeyLayout layout, RenderOptions options)
    {
        if (!spawnOnKeys) return;
        var fullLeft = layout.keys[0].left;
        var fullRight = layout.keys[0].left;
        var fullWidth = layout.whiteKeyWidth;

        if (uiSpawnrate.Value < 0.005) uiSpawnrate.Value = 0;

        var simSpeed = uiSimulationSpeed.Value / uiSimulationSubsteps.Value;

        var aspect = options.renderAspectRatio;
        var timeFactor = 1.0 / options.renderFPS * simSpeed;

        var baseRate = uiSpawnrate.Value / (1 - uiSpawnrate.Value);
        baseRate *= simSpeed;

        for (int substep = 0; substep < uiSimulationSubsteps.Value; substep++)
        {
            for (int i = firstKey; i < lastKey; i++)
            {
                if (pressed[i] > 0)
                {
                    var rate = Math.Sqrt(pressed[i]) * baseRate;
                    rate = rate / (1 + rate);
                    while (rate > r.NextDouble())
                    {
                        var left = layout.notes[i].left;
                        var right = layout.notes[i].right;

                        left = (left - fullLeft) / fullWidth;
                        right = (right - fullLeft) / fullWidth;

                        var x = (left + right) / 2;
                        x += (r.NextDouble() - 0.5) * (right - left) * uiSpawnAreaWidth.Value;

                        SpawnParticle(new Vector2d(x, 0), colors[i * 2]);
                    }
                }
            }
            StepSimulation(timeFactor, aspect);
        }
    }

    public void GeneratUnderNotes(IEnumerable<Note> notes, double keyboardHeight, int firstKey, int lastKey, KeyLayout layout, RenderOptions options)
    {
        if (!(spawnUnderNotes || spawnAboveNotes)) return;
        var fullLeft = layout.keys[0].left;
        var fullRight = layout.keys[0].left;
        var fullWidth = layout.whiteKeyWidth;

        if (uiSpawnrate.Value < 0.005) uiSpawnrate.Value = 0;

        var simSpeed = uiSimulationSpeed.Value / uiSimulationSubsteps.Value;

        double notePosFactor = 1 / options.noteScreenTime;
        double renderCutoff = options.midiTime + options.noteScreenTime;

        var aspect = options.renderAspectRatio;
        var timeFactor = 1.0 / options.renderFPS * simSpeed;

        var baseRate = uiSpawnrate.Value / (1 - uiSpawnrate.Value);
        baseRate *= simSpeed;

        for (int substep = 0; substep < uiSimulationSubsteps.Value; substep++)
        {
            foreach (var note in notes)
            {
                var left = layout.notes[note.key].left;
                var right = layout.notes[note.key].right;

                left = (left - fullLeft) / fullWidth;
                right = (right - fullLeft) / fullWidth;

                double top = (1 - (renderCutoff - note.end) * notePosFactor) * (1 - keyboardHeight);
                double bottom = (1 - (renderCutoff - note.start) * notePosFactor) * (1 - keyboardHeight);
                if (!note.hasEnded) top = 1.2;
                top /= fullWidth;
                bottom /= fullWidth;

                if (spawnAboveNotes) bottom = top;

                var rate = baseRate;
                var area = (right - left) * (top - bottom);
                if (!spawnAboveNotes && area > 0) rate *= area * keyboardHeight;
                rate = rate / (1 + rate);
                while (rate > r.NextDouble())
                {
                    var x = (left + right) / 2;
                    x += (r.NextDouble() - 0.5) * (right - left) * uiSpawnAreaWidth.Value;

                    var y = (top + bottom) / 2;
                    y += (r.NextDouble() - 0.5) * (top - bottom);

                    SpawnParticle(new Vector2d(x, y), note.color.left);
                }
            }
            StepSimulation(timeFactor, aspect);
        }
    }

    public void RenderUnderNotes(KeyLayout layout, double keyboardHeight, RenderOptions options)
    {
        if (!renderUnderNotes) return;
        Render(layout, keyboardHeight, options);
    }

    public void RenderUnderKeys(KeyLayout layout, double keyboardHeight, RenderOptions options)
    {
        if (!renderUnderKeys) return;
        Render(layout, keyboardHeight, options);
    }

    public void RenderOverKeys(KeyLayout layout, double keyboardHeight, RenderOptions options)
    {
        if (!renderOnKeys) return;
        Render(layout, keyboardHeight, options);
    }

    public void Render(KeyLayout layout, double keyboardHeight, RenderOptions options)
    {
        if (uiShader.Checked) IO.SetBlendFunc(BlendFunc.Add);
        var fullLeft = layout.keys[0].left;
        var fullRight = layout.keys[0].left;
        var fullWidth = layout.whiteKeyWidth;

        var aspect = options.renderAspectRatio;
        var node = particles.First;
        Texture tex = null;
        switch (uiParticleTex.Value)
        {
            case "Blob": tex = prtKeyBlob; break;
            case "Spark": tex = prtKeySpark; break;
            case "Square": tex = prtKeySquare; break;
            case "Circle": tex = prtKeyCircle; break;
        }
        while (node != null)
        {
            var p = node.Value;

            var pos = p.pos;
            pos *= fullWidth;

            var width = layout.whiteKeyWidth;
            var lifeSize = 1 - Math.Max(0, uiParticleLifeSize.Value - p.life) / uiParticleLifeSize.Value;
            if (uiParticleLifeSize.Value < 0.001) lifeSize = 1;
            var size = p.size * lifeSize;

            pos.X += fullLeft;
            pos.Y += keyboardHeight;

            if (uiCollide.Checked) pos.Y += size * uiCollideSize.Value * width;

            double cos = Math.Cos(p.rotation);
            double sin = Math.Sin(p.rotation);

            var lifeOpacity = 1 - Math.Max(0, uiParticleLifeFade.Value - p.life) / uiParticleLifeFade.Value;
            if (uiParticleLifeFade.Value < 0.001) lifeOpacity = 1;

            var col = p.color;
            col = Util.BlendColors(col, new Color4(1, 1, 1, (float)uiLightness.Value));
            col.A *= (float)uiOpacity.Value;
            col.A *= (float)lifeOpacity;


            IO.RenderShape(
                pos + new Vector2d(cos, sin * aspect) * width * size,
                pos + new Vector2d(-sin, cos * aspect) * width * size,
                pos - new Vector2d(cos, sin * aspect) * width * size,
                pos - new Vector2d(-sin, cos * aspect) * width * size,
                col,
                tex
            );

            if (pos.Y < 0) p.life = 0;

            var _node = node.Next;
            if (p.life <= 0) particles.Remove(node);
            node = _node;
        }
        if (uiShader.Checked) IO.SetBlendFunc(BlendFunc.Mix);
    }

    public void Reset()
    {
        particles.Clear();
    }
    }

//---------Key Particle---------

    class KeyParticle
    {
    public Vector2d pos;
    public Vector2d vel;

    public Color4 color;

    public double rotation;
    public double rotationVel;
    public double size;

    public double life;

    public double streakExcess = 0;

    public KeyParticle(Vector2d pos, double rotation, double rotVel, Vector2d vel, Color4 color, double size, double life)
    {
        this.pos = pos;
        this.vel = vel;

        this.life = life;
        this.rotation = rotation;
        this.rotationVel = rotVel;
        this.size = size;
        this.color = color;
    }

    public void Push(double delta, Vector2d acc)
    {
        vel += acc * delta * 60;
    }


    public void PushRot(double delta, double acc)
    {
        rotationVel += acc * delta * 60;
    }

    public void Step(double delta, double aspect)
    {
        life -= delta;
        pos.X += vel.X * delta * 60 / aspect;
        pos.Y += vel.Y * delta * 60;
        rotation -= rotationVel * delta * 60;
    }
    }

//---------Noise Maker---------

    public class NoiseMaker
    {
    private static int[] p = new int[512];
    private static int[] permutation = { 151,160,137,91,90,15,
               131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
               190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
               88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
               77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
               102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
               135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
               5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
               223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
               129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
               251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
               49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
               138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
               };

    static NoiseMaker()
    {
        CalculateP();
    }

    private static int _octaves;
    private static int _halfLength = 256;

    public static void SetOctaves(int octaves)
    {
        _octaves = octaves;

        var len = (int)Math.Pow(2, octaves);

        permutation = new int[len];

        Reseed();
    }

    private static void CalculateP()
    {
        p = new int[permutation.Length * 2];
        _halfLength = permutation.Length;

        for (int i = 0; i < permutation.Length; i++)
            p[permutation.Length + i] = p[i] = permutation[i];
    }

    public static void Reseed()
    {
        var random = new Random();
        var perm = Enumerable.Range(0, permutation.Length).ToArray();

        for (var i = 0; i < perm.Length; i++)
        {
            var swapIndex = random.Next(perm.Length);

            var t = perm[i];

            perm[i] = perm[swapIndex];

            perm[swapIndex] = t;
        }

        permutation = perm;

        CalculateP();

    }

    public static double Noise(Vector3 position, int octaves, ref double min, ref double max)
    {
        return Noise(position.X, position.Y, position.Z, octaves, ref min, ref max);
    }

    public static double Noise(double x, double y, double z, int octaves, ref double min, ref double max)
    {

        var perlin = 0.0;
        var octave = 1;

        for (var i = 0; i < octaves; i++)
        {
            var noise = Noise(x * octave, y * octave, z * octave);

            perlin += noise / octave;

            octave *= 2;
        }

        perlin = Math.Abs((double)Math.Pow(perlin, 2));
        max = Math.Max(perlin, max);
        min = Math.Min(perlin, min);

        return perlin;
    }

    public static double Noise(double x, double y, double z)
    {
        int X = (int)Math.Floor(x) % _halfLength;
        int Y = (int)Math.Floor(y) % _halfLength;
        int Z = (int)Math.Floor(z) % _halfLength;

        if (X < 0)
            X += _halfLength;

        if (Y < 0)
            Y += _halfLength;

        if (Z < 0)
            Z += _halfLength;

        x -= (int)Math.Floor(x);
        y -= (int)Math.Floor(y);
        z -= (int)Math.Floor(z);

        var u = Fade(x);
        var v = Fade(y);
        var w = Fade(z);

        int A = p[X] + Y, AA = p[A] + Z, AB = p[A + 1] + Z,
            B = p[X + 1] + Y, BA = p[B] + Z, BB = p[B + 1] + Z;


        return Lerp(
                Lerp(
                    Lerp(Grad(p[AA], x, y, z), Grad(p[BA], x - 1, y, z), u),
                    Lerp(Grad(p[AB], x, y - 1, z), Grad(p[BB], x - 1, y - 1, z), u),
                    v
                ),
                Lerp(
                    Lerp(Grad(p[AA + 1], x, y, z - 1), Grad(p[BA + 1], x - 1, y, z - 1), u),
                    Lerp(Grad(p[AB + 1], x, y - 1, z - 1), Grad(p[BB + 1], x - 1, y - 1, z - 1), u),
                    v
                ),
                w
            );

    }

    static double Fade(double t) { return t * t * t * (t * (t * 6 - 15) + 10); }

    static double Grad(int hash, double x, double y, double z)
    {
        int h = hash & 15;

        double u = h < 8 ? x : y,
               v = h < 4 ? y : h == 12 || h == 14 ? x : z;

        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    static double Lerp(double first, double second, double by)
    {
        return first * (1 - by) + second * by;
    }

    }