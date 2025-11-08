using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Text;

namespace jumpscare
{
    internal sealed class ModEntry : Mod
    {
        private Random random = new();

        private readonly int chance = 100_000;
        private readonly int chestChance = 100;

        private SoundEffect scream = null!;
        private Texture2D spriteSheet = null!;

        private int frameWidth = 200;
        private int frameHeight = 150;
        private int frameCount = 14;

        private int currentFrame = 0;
        private double frameTimer = 0;
        private double frameDuration = 50;
        private bool playing = false;

        //DEBUG trigger: Attack
        private readonly string cheatCode = "BAGOWPG";
        private StringBuilder inputBuffer = new();

        public override void Entry(IModHelper helper)
        {
            random = new Random();

            string assetsPath = Path.Combine(helper.DirectoryPath, "assets");
            string spritePath = Path.Combine(assetsPath, "foxy.png");
            string soundPath = Path.Combine(assetsPath, "foxy.wav");

            spriteSheet = helper.ModContent.Load<Texture2D>(spritePath);
            scream = SoundEffect.FromFile(soundPath);

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Display.Rendered += this.OnRendered;
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            //ignore if not in game
            if (!Context.IsWorldReady)
                return;

            if (e.Button == SButton.MouseRight)
            {
                var cursorTile = e.Cursor.Tile;
                try
                {
                    if (Game1.currentLocation.objects.TryGetValue(cursorTile, out StardewValley.Object obj))
                    {
                        if (obj is Chest chest)
                        {
                            if (random.Next(chestChance) == 0)
                                StartScare("Chest"); 
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"{ex.Message}", LogLevel.Debug);
                }
            }

            if (e.Button.ToString().Length == 1 && char.IsLetter(e.Button.ToString()[0]))
            {
                inputBuffer.Append(e.Button.ToString().ToUpper());

                if (inputBuffer.Length > cheatCode.Length)
                    inputBuffer.Remove(0, inputBuffer.Length - cheatCode.Length);

                if (inputBuffer.ToString().EndsWith(cheatCode))
                {
                    StartScare("Debug");
                    inputBuffer.Clear();
                }
            }

            this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            //ignore if not in game
            if (!Context.IsWorldReady)
                return;
            
            if (random.Next(chance) == 0)
            {
                StartScare("Random");
            }
        }
        private void StartScare(string source)
        {
            scream.CreateInstance().Play();
            playing = true;
            currentFrame = 0;
            frameTimer = 0;
            this.Monitor.Log($"{source} scream", LogLevel.Debug);
        }

        private void OnRendered(object? sender, RenderedEventArgs e)
        {
            if (!playing) return;

            frameTimer += Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
            if (frameTimer >= frameDuration)
            {
                frameTimer = 0;
                currentFrame++;
                if (currentFrame >= frameCount)
                {
                    playing = false;
                    return;
                }
            }

            int cols = spriteSheet.Width / frameWidth;
            int x = (currentFrame % cols) * frameWidth;
            int y = (currentFrame / cols) * frameHeight;
            Rectangle source = new Rectangle(x, y, frameWidth, frameHeight);

            e.SpriteBatch.Draw(
                spriteSheet,
                new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height),
                source,
                Color.White
            );
        }
    }
}