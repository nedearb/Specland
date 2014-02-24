﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Specland {
    public class World {

        public string name = "";

        public Rectangle viewPortInTiles = new Rectangle();
        public Rectangle viewPortInPixels = new Rectangle();
        public Point viewOffset = new Point(0, 0);

        public static int tileSizeInPixels = 16;

        public static int WALLDEPTH = 1;
        public static int TILEDEPTH = 0;

        public static int previousUniqueID = 0;

        public Point sizeInTiles;

        public int[, ,] TileMatrix;
        public int[,] LiquidMatrix;

        public bool[,] LiquidNeedsUpdateMatrix;

        public bool[, ,] TileNeedsUpdateMatrix;

        public int[,] LightMatrix;
        public int[,] TempLightMatrix;

        public byte[,] CrackMatrix;

        public int[] skyY;



        //public Rectangle[,] drawRectangles;
        public TextureInfo[,] textureTileInfo;
        public TextureInfo[,] textureWallInfo;
        public TextureInfo[,] textureLiquidInfo;

        public int skyLightBrightness = 0;

        public List<Entity> EntityList = new List<Entity>();
        public List<Entity> EntityRemovalList = new List<Entity>();
        public List<Entity> EntityAddingList = new List<Entity>();

        public double time = 7000;

        public EntityPlayer player;

        public bool lightingNeedsUpdate = false;
        public FpsControl lightingThreadFps;

        Rectangle dr = new Rectangle(0, 0, tileSizeInPixels, tileSizeInPixels);

        private Color[] grayColors = new Color[256];

        public World(Point size, string name) {
            this.sizeInTiles = size;
            TileMatrix = new int[size.X, size.Y, 2];
            LiquidMatrix = new int[size.X, size.Y];
            //drawRectangles = new Rectangle[size.X, size.Y];
            LightMatrix = new int[size.X, size.Y];
            TempLightMatrix = new int[size.X, size.Y];
            CrackMatrix = new byte[size.X, size.Y];
            TileNeedsUpdateMatrix = new bool[size.X, size.Y, 2];
            LiquidNeedsUpdateMatrix = new bool[size.X, size.Y];
            skyY = new int[size.X];

            lightingThreadFps = new FpsControl();

            for(int i=0;i<256;i++){
                grayColors[i] = new Color((byte)i, (byte)i, (byte)i);
            }
        }

        public bool calculateTileFrames(Game game) {

            textureTileInfo = new TextureInfo[sizeInTiles.X, sizeInTiles.Y];
            textureWallInfo = new TextureInfo[sizeInTiles.X, sizeInTiles.Y];
            textureLiquidInfo = new TextureInfo[sizeInTiles.X, sizeInTiles.Y];

            for (int x = 0; x < sizeInTiles.X; x++) {
                setSkyTile(x);
                for (int y = 0; y < sizeInTiles.Y; y++) {
                    calculateTileFrame(game, x, y, true);
                    calculateTileFrame(game, x, y, false);
                    calculateLiquidFrame(game, x, y);
                    lightUpdate1(x, y);
                }
            }
            for (int x = 0; x < sizeInTiles.X; x++) {
                lightUpdate2(x, 0, true);
                for (int y = 1; y < sizeInTiles.Y; y++) {
                    lightUpdate2(x, y, false);
                }
            }

            LightMatrix = (int[,])TempLightMatrix.Clone();

            return true;

        }

        private void calculateTileFrame(Game game, int x, int y, bool isWall) {
            if ((isWall?textureTileInfo:textureWallInfo) != null) {
                Tile tile = getTileObject(x, y, isWall);
                if (tile != null && inWorld(x, y)) {
                    if(isWall){
                        textureWallInfo[x, y] = tile.getTextureInfo(x, y, this, true);
                    }else{
                        textureTileInfo[x, y] = tile.getTextureInfo(x, y, this, false);
                    }
                }
            }
        }

        private void calculateLiquidFrame(Game game, int x, int y) {
            if (textureLiquidInfo != null) {
                if (inWorld(x, y)) {
                    textureLiquidInfo[x, y] = getLiquidTextureInfo(x, y);
                }
            }
        }

        public int getLight(int x, int y) {
            if(inWorld(x, y)){
                return LightMatrix[x, y];
            }
            return 0;
        }

        private TextureInfo getLiquidTextureInfo(int x, int y) {
            int a = (int)Math.Ceiling(MathHelper.Clamp(LiquidMatrix[x, y], 0, 100) / (100.0f/8.0f));
            return new TextureInfo(/*Tile.TextureLiquidWater,*/ new Rectangle(((a%3) * 8), (64)+((a/3) * 8), 8, 8), true);
        }

        public Tile getTileObject(int x, int y, bool isWall) {
            Tile t = Tile.getTileObject(getTileIndex(x, y, isWall));
            return (t!=null?t:Tile.TileAir);
        }

        public Tile getTileObjectNoCheck(int x, int y, bool isWall) {
            Tile t = Tile.getTileObject(getTileIndexNoCheck(x, y, isWall));
            return (t != null ? t : Tile.TileAir);
        }

        public int getTileIndex(int x, int y, bool isWall) {
            if(inWorld(x, y)){
                return TileMatrix[x, y, isWall ? WALLDEPTH : TILEDEPTH];
            }
            return Tile.TileDirt.index;
        }

        public int getTileIndexNoCheck(int x, int y, bool isWall) {
            return TileMatrix[x, y, isWall ? WALLDEPTH : TILEDEPTH];
        }

        internal void setTileForUpdate(int x, int y, bool isWall) {
            if (inWorld(x, y)) {
                TileNeedsUpdateMatrix[x, y, isWall ? WALLDEPTH : TILEDEPTH] = true;
            }
        }

        public int getLiquid(int x, int y) {
            if (inWorld(x, y)) {
                return LiquidMatrix[x, y];
            }
            return 0;
        }

        public int getLiquidNoCheck(int x, int y) {
            return LiquidMatrix[x, y];
        }

        public bool setLiquid(int x, int y, int l) {
            if (inWorld(x, y)) {
                LiquidMatrix[x, y] = l;
                calculateLiquidFrame(Game.instance, x, y);
                calculateLiquidFrame(Game.instance, x + 1, y);
                calculateLiquidFrame(Game.instance, x - 1, y);
                calculateLiquidFrame(Game.instance, x, y + 1);
                calculateLiquidFrame(Game.instance, x, y - 1);
                lightingNeedsUpdate = true;
                return true;
            }
            return false;
        }

        public bool mineTile(int x, int y, ItemPick pick, bool isWall) {
            if (mineTileNoNearUpdate(x, y, pick, isWall)) {
                setTileForUpdate(x - 1, y, isWall);
                setTileForUpdate(x + 1, y, isWall);
                setTileForUpdate(x, y - 1, isWall);
                setTileForUpdate(x, y + 1, isWall);
                return true;
            }
            return false;
        }

        public bool setTileWithUpdate(int x, int y, int index, bool isWall) {
            if (setTile(x, y, index, isWall)) {
                TileNeedsUpdateMatrix[x, y, isWall ? WALLDEPTH : TILEDEPTH] = true;
                setTileForUpdate(x - 1, y, isWall);
                setTileForUpdate(x + 1, y, isWall);
                setTileForUpdate(x, y - 1, isWall);
                setTileForUpdate(x, y + 1, isWall);
                return true;
            }
            return false;
        }


        public bool mineTileNoNearUpdate(int x, int y, ItemPick pick, bool isWall) {
            Tile tile = getTileObject(x, y, isWall);
            if (setTile(x, y, Tile.TileAir.index, isWall)) {
                tile.mine(this, x, y, pick, isWall);
                Entity e = new EntityItem(new Vector2((x) * World.tileSizeInPixels, (y) * World.tileSizeInPixels), tile.dropStack(pick, Game.rand));
                EntityList.Add(e);
                return true;
            }
            return false;
        }

        public bool setTile(int x, int y, int index, bool isWall) {
            if (inWorld(x, y)) {
                TileMatrix[x, y, isWall ? WALLDEPTH : TILEDEPTH] = index;
                calculateTileFrame(Game.instance, x, y, isWall);
                calculateTileFrame(Game.instance, x + 1, y, isWall);
                calculateTileFrame(Game.instance, x - 1, y, isWall);
                calculateTileFrame(Game.instance, x, y + 1, isWall);
                calculateTileFrame(Game.instance, x, y - 1, isWall);
                setSkyTile(x);
                lightingNeedsUpdate = true;
                return true;
            }
            return false;
        }

        public void setCrackNoCheck(int x, int y, byte amount) {
            CrackMatrix[x, y] = amount;
        }

        public byte getCrackNoCheck(int x, int y) {
            return CrackMatrix[x, y];
        }

        private int getCrack(int x, int y) {
            if (inWorld(x, y)) {
                return CrackMatrix[x, y];
            } else {
                return 0;
            }
        }

        private void setSkyTile(int x) {
            int i;
            for (i = 0; i < sizeInTiles.Y;i++ ) {
                if (!getTileObject(x, i, false).transparent) {
                    break;
                }
            }
            skyY[x] = i;
        }

        public bool setTile(int x, int y, Tile tile, bool isWall) {
            return tile == null ? false : setTile(x, y, tile.index, isWall);
        }

        public bool inWorld(int x, int y) {
            return x >= 0 && x < sizeInTiles.X && y >= 0 && y < sizeInTiles.Y;
        }


        public void Update(Game game) {

            foreach (Entity entity in EntityList) {
                entity.update(game, this);
            }

            viewOffset.X = (int)player.displayPosition.X - (viewPortInPixels.Width / 2);
            viewOffset.Y = (int)player.displayPosition.Y - (viewPortInPixels.Height / 2);

            if (viewOffset.X < 0) {
                viewOffset.X = 0;
            }
            if (viewOffset.Y < 0) {
                viewOffset.Y = 0;
            }
            if (viewOffset.X + viewPortInPixels.Width > sizeInTiles.X * tileSizeInPixels) {
                viewOffset.X = (sizeInTiles.X * tileSizeInPixels) - viewPortInPixels.Width;
            }
            if (viewOffset.Y + viewPortInPixels.Height > sizeInTiles.Y * tileSizeInPixels) {
                viewOffset.Y = (sizeInTiles.Y * tileSizeInPixels) - viewPortInPixels.Height;
            }

            viewPortInPixels.X = viewOffset.X;
            viewPortInPixels.Y = viewOffset.Y;
            viewPortInPixels.Width = game.Window.ClientBounds.Width;
            viewPortInPixels.Height = game.Window.ClientBounds.Height;

            viewPortInTiles = divideRectangle(viewPortInPixels, tileSizeInPixels);
            viewPortInTiles.Width += 1;
            viewPortInTiles.Height += 1;

            //TempLiquidMatrix = (int[,])LiquidMatrix.Clone();

            int border = 16;

            for (int x = viewPortInTiles.X - border; x < viewPortInTiles.X + viewPortInTiles.Width + 1 + border; x++) {
                for (int y = viewPortInTiles.Y + viewPortInTiles.Height + 1 + border; y > viewPortInTiles.Y - border; y--) {
                    if (inWorld(x, y)) {
                        if (getCrackNoCheck(x, y) > 0) {
                            CrackMatrix[x, y]--;
                        }
                        if (LiquidNeedsUpdateMatrix[x, y]) {
                            updateLiquid(x, y);
                            LiquidNeedsUpdateMatrix[x, y] = false;
                        }
                        if (TileNeedsUpdateMatrix[x, y, TILEDEPTH]) {
                            getTileObjectNoCheck(x, y, false).updateNearChange(this, x, y, false);
                            TileNeedsUpdateMatrix[x, y, TILEDEPTH] = false;
                            LiquidNeedsUpdateMatrix[x, y] = true;
                        }
                        if (TileNeedsUpdateMatrix[x, y, WALLDEPTH]) {
                            getTileObjectNoCheck(x, y, true).updateNearChange(this, x, y, true);
                            TileNeedsUpdateMatrix[x, y, WALLDEPTH] = false;
                        }
                    }
                    /*if (x >= viewPortInTiles.X && y >= viewPortInTiles.Y && x <= viewPortInTiles.X + viewPortInTiles.Width + 1 && y <= viewPortInTiles.Y + viewPortInTiles.Height + 1) {
                        if (inWorld(x, y)) {
                            
                        }
                    }*/
                }
            }

            for (int i = 0; i < 100; i++) {
                int x = Game.rand.Next(sizeInTiles.X);
                int y = Game.rand.Next(sizeInTiles.Y);
                bool d = Game.rand.Next()%2==0;
                getTileObjectNoCheck(x, y, d).updateRandom(this, x, y, d);
            }

            while (EntityRemovalList.Count()>0) {
                EntityList.Remove(EntityRemovalList[0]);
                EntityRemovalList.RemoveAt(0);
            }

            while (EntityAddingList.Count() > 0) {
                EntityList.Add(EntityAddingList[0]);
                EntityAddingList.RemoveAt(0);
            }


            if (time > 5000 && time < 8000) {
                skyLightBrightness = (int)(((time - 5000) / 3000.0) * 300);
            }

            if (time > 16000 && time < 19000) {
                skyLightBrightness = (int)(((1.0 - (time - 16000) / 3000.0)) * 300);
            }

            if (skyLightBrightness<16) {
                skyLightBrightness = 16;
            }

            if(time>24000){
                time -=24000;
            }
            time += .1;
            lightingNeedsUpdate = true;
        }

        private void updateLiquid(int x, int y) {
            if (!getTileObject(x, y + 1, false).solid)LiquidNeedsUpdateMatrix[x, y + 1] = true;
            if (!getTileObject(x+1, y, false).solid) LiquidNeedsUpdateMatrix[x + 1, y] = true;
            if (!getTileObject(x-1, y, false).solid) LiquidNeedsUpdateMatrix[x - 1, y] = true;
            if(!(inWorld(x, y) && inWorld(x, y+1))){
                return;
            }
            int l = LiquidMatrix[x, y];
            int b = LiquidMatrix[x, y+1];

            if(l>0){
                if (!getTileObject(x, y + 1, false).solid) {
                    while(l>0 && b<100){
                        l--;
                        b++;
                    }
                    setLiquid(x, y, l);
                    setLiquid(x, y + 1, b);
                }
                l = LiquidMatrix[x, y];
                b = LiquidMatrix[x, y + 1];
                if (getTileObject(x, y + 1, false).solid || b >= 50) {
                    bool ls = getTileObject(x - 1, y, false).solid;
                    bool rs = getTileObject(x + 1, y, false).solid;

                    int lb = getLiquid(x - 1, y);
                    int rb = getLiquid(x + 1, y);

                    if ((!ls && lb < 100) && (!rs && rb < 100)) {

                        while ((l > rb || l > lb) && l > 0) {
                            if (l > 0 && l > lb) {
                                l--;
                                lb++;
                            }
                            if (l > 0 && l > rb) {
                                l--;
                                rb++;
                            }
                        }

                        setLiquid(x, y, l);
                        setLiquid(x - 1, y, lb);
                        setLiquid(x + 1, y, rb);

                    } else if ((!rs && rb < 100)) {

                        if (!inWorld(x + 1, y)) {
                            return;
                        }

                        while (l > 0 && l > rb) {
                            l--;
                            rb++;
                        }

                        setLiquid(x, y, l);
                        setLiquid(x + 1, y, rb);

                    } else if ((!ls && lb < 100)) {

                        while (l > 0 && l > lb) {
                            l--;
                            lb++;
                        }

                        setLiquid(x, y, l);
                        setLiquid(x - 1, y, lb);

                    }
                }
            }
        }

        public void Draw(Game game, GameTime gameTime) {

            Profiler.start("draw tiles");
            for (int x = viewPortInTiles.X; x < viewPortInTiles.X + viewPortInTiles.Width + 3; x++) {
                for (int y = viewPortInTiles.Y; y < viewPortInTiles.Y + viewPortInTiles.Height + 3; y++) {
                    if (inWorld(x, y)) {
                        Tile tile = getTileObjectNoCheck(x, y, false);
                        Tile wall = getTileObjectNoCheck(x, y, true);
                        dr.X = (tileSizeInPixels * x) - viewOffset.X;
                        dr.Y = (tileSizeInPixels * y) - viewOffset.Y;
                        if (textureTileInfo[x, y].transparent && wall.renderType != Tile.RenderTypeNone) {
                            byte b = (byte)Clamp(LightMatrix[x, y] - (255 - wall.wallBrightness), 0, wall.wallBrightness);
                            drawRect(game, Tile.TileSheet, dr, textureWallInfo[x, y].rectangle, grayColors[b]);
                        }
                        byte a = (byte)Clamp(LightMatrix[x, y], 0, 255);
                        if (textureTileInfo[x, y].transparent) {
                            if (LiquidMatrix[x, y] > 0 && textureLiquidInfo[x, y] != null) {
                                drawRect(game, Tile.TileSheet, dr, textureLiquidInfo[x, y].rectangle, grayColors[a]);
                            }
                        }
                        if(tile.index != Tile.TileAir.index){
                            if (textureTileInfo[x, y] != null) {
                                drawRect(game, Tile.TileSheet, dr, textureTileInfo[x, y].rectangle, grayColors[a]);
                            }
                        }
                    }
                }
            }
            Profiler.end("draw tiles");

            Profiler.start("draw entities");
            foreach (Entity entity in EntityList) {
                entity.draw(game, this);
            }
            Profiler.end("draw entities");

        }

        private void drawRect(Game game, Texture2D texture, Rectangle destination, Rectangle source, Color color) {
            game.spriteBatch.Draw(texture, destination, source, color);
        }

        private Rectangle subtractPointToRectangle(Point point, Rectangle rectangle) {
            return new Rectangle(rectangle.X-point.X, rectangle.Y-point.Y, rectangle.Width, rectangle.Height);
        }

        private Rectangle divideRectangle(Rectangle rectangle, int number) {
            return new Rectangle(rectangle.X / number, rectangle.Y / number, rectangle.Width / number, rectangle.Height / number);
        }

        internal void lightingThreadUpdate() {

            if (lightingNeedsUpdate) {
                Profiler.start("lighting");

                TempLightMatrix = (int[,])LightMatrix.Clone();

                int border = 16;

                for (int x = viewPortInTiles.X - border; x < viewPortInTiles.X + viewPortInTiles.Width + 1 + border; x++) {
                    for (int y = viewPortInTiles.Y - border; y < viewPortInTiles.Y + viewPortInTiles.Height + 1 + border; y++) {
                        lightUpdate1(x, y);
                    }
                }

                for (int x = viewPortInTiles.X - border; x < viewPortInTiles.X + viewPortInTiles.Width + 1 + border; x++) {
                    lightUpdate2(x, viewPortInTiles.Y - border, true);
                    for (int y = viewPortInTiles.Y + 1 - border; y < viewPortInTiles.Y + viewPortInTiles.Height + 1 + border; y++) {
                        lightUpdate2(x, y, false);
                    }
                }

                LightMatrix = (int[,])TempLightMatrix.Clone();

                lightingNeedsUpdate = false;

                Profiler.end("lighting");

                lightingThreadFps.update();
            }
        }

        private void lightUpdate1(int x, int y) {
            if (inWorld(x, y)) {
                TempLightMatrix[x, y] = 0;
            }
        }

        private void lightUpdate2(int x, int y, bool firstY) {
            if (inWorld(x, y)) {
                //if (firstY && y < skyY[x]) {
                if (textureTileInfo[x, y].transparent && textureWallInfo[x, y].transparent) {
                    setLightFromSource(x, y, true, skyLightBrightness);
                }
                Tile tile = getTileObject(x, y, false);
                Tile wall = getTileObject(x, y, true);
                if (tile.light > 0 || wall.light > 0) {
                    setLightFromSource(x, y, false, Math.Max(tile.light, wall.light));
                }
            }
        }

        private void setLightFromSource(int x, int y, bool isSkyLight, int power) {
            spreadLight(x, y, power, 0, isSkyLight, false);
            spreadLight(x, y, power, 1, isSkyLight, true);
            spreadLight(x, y, power, 2, isSkyLight, false);
            spreadLight(x, y, power, 3, isSkyLight, true);
        }

        private void spreadLight(int x, int y, int a, int q, bool isSkyLight, bool d) {
            if (inWorld(x, y)) {

                int b = a;

                //bool t = false;

                if (TempLightMatrix[x, y] < b) {
                    TempLightMatrix[x, y] = b;
                } else {
                    return;
                }

                if (getTileObjectNoCheck(x, y, false).transparent) {
                    b = a - 10;
                    //t = true;
                } else {
                    b = a - 100;
                }

                if (b > 0) {
                    //if (q == 0 || ((q == 1 || q == 3) && d)) {
                        spreadLight(x - 1, y, b, 0, isSkyLight, d);//left
                    //}
                    //if (q == 1 || ((q == 0 || q == 2) && !d)) {
                        //spreadLight(x, y + 1, (isSkyLight && t) ? a : b, 1, isSkyLight, d);//down
                        spreadLight(x, y + 1, b, 1, isSkyLight, d);//down
                    //}
                    //if (q == 2 || ((q == 1 || q == 3) && d)) {
                        spreadLight(x + 1, y, b, 2, isSkyLight, d);//right
                    //}
                    //if (q == 3 || ((q == 0 || q == 2) && !d)) {
                        spreadLight(x, y - 1, b, 3, isSkyLight, d);//up
                    //}
                }
            }
        }

        public static int Clamp(int value, int min, int max) {
            return value > max ? max : (value < min ? min : (value));
        }

        public Color getBgColor() {
            float a = (MathHelper.Clamp(skyLightBrightness, 0, 255))/255.0f;
            return new Color((byte)(100*a), (byte)(149*a), (byte)(237*a));
        }

        public void generate(int type) {
            WorldGenerator.Generate(this, type);
        }

        public static long getnewUniqueID() {
            previousUniqueID++;
            return previousUniqueID;
        }

        public static bool save(World world, string name, bool forceOverride = true){

            String file = Game.saveLocation + @"\" + name + "." + Game.saveExtention;

            if (System.IO.File.Exists(file) && !forceOverride) {
                return false;
            }

            List<Byte> bytes = new List<byte>();
            byte[] xBytes = intToBytes(world.sizeInTiles.X);
            byte[] yBytes = intToBytes(world.sizeInTiles.Y);

            bytes.Add(xBytes[0]);
            bytes.Add(xBytes[1]);
            bytes.Add(xBytes[2]);
            bytes.Add(xBytes[3]);

            bytes.Add(yBytes[0]);
            bytes.Add(yBytes[1]);
            bytes.Add(yBytes[2]);
            bytes.Add(yBytes[3]);

            byte[] timeBytes = intToBytes((int)world.time);

            bytes.Add(timeBytes[0]);
            bytes.Add(timeBytes[1]);
            bytes.Add(timeBytes[2]);
            bytes.Add(timeBytes[3]);

            for (int x = 0; x < world.sizeInTiles.X; x++) {
                for (int y = 0; y < world.sizeInTiles.Y; y++) {
                    bytes.Add((byte)world.TileMatrix[x, y, 0]);
                    bytes.Add((byte)world.TileMatrix[x, y, 1]);
                    bytes.Add((byte)world.LiquidMatrix[x, y]);
                }
            }
            world.player.saveTo(bytes);
            byte[] byteArray = bytes.ToArray<byte>();
            System.IO.File.WriteAllBytes(file, byteArray);
            return true;
        }

        public static World load(string name) {
            byte[] bytes = System.IO.File.ReadAllBytes(Game.saveLocation + @"\" + name + "." + Game.saveExtention);

            World world = new World(new Point(bytesToInt(bytes, 0), bytesToInt(bytes, 4)), name);
            world.time = bytesToInt(bytes, 8);
            
            int index = 12;
            for (int x = 0; x < world.sizeInTiles.X; x++) {
                for (int y = 0; y < world.sizeInTiles.Y; y++) {
                    world.TileMatrix[x, y, 0] = bytes[index];
                    index++;
                    world.TileMatrix[x, y, 1] = bytes[index];
                    index++;
                    world.LiquidMatrix[x, y] = bytes[index];
                    index++;
                }
            }
            world.player = new EntityPlayer(0, 0);
            world.player.loadFrom(bytes, index);
            world.EntityList.Add(world.player);
            world.calculateTileFrames(Game.instance);

            return world;
        }

        public static byte[] intToBytes(int value) {
            return new byte[4] { 
                    (byte)(value & 0xFF), 
                    (byte)((value >> 8) & 0xFF), 
                    (byte)((value >> 16) & 0xFF), 
                    (byte)((value >> 24) & 0xFF) };
        }

        public static int bytesToInt(byte[] value, int index) {
            return (int)(
                value[0 + index] << 0 |
                value[1 + index] << 8 |
                value[2 + index] << 16 |
                value[3 + index] << 24);
        }
    }
}