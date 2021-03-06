﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Specland {

    public class Tile {

        public enum RenderType {
            none, terrain, placed, building, alone, platform, custom
        }

        public enum Material {
            none, dirt, stone, wood, furniture, SIZE
        }

        public static Texture2D TileSheet;

        public static Tile[] TileList = new Tile[100];

        public static Tile TileAir = new Tile("Air", RenderType.none, Material.none, 0, 0).setTransparent().notSolid();
        public static Tile TileGrass = new TileGrass("Grass", RenderType.terrain, Material.dirt, 0, 0);
        public static Tile TileDirt = new Tile("Dirt", RenderType.terrain, Material.dirt, 1, 0);
        public static Tile TileStone = new Tile("Stone", RenderType.terrain, Material.stone, 2, 0);
        public static Tile TileStoneBricks = new Tile("StoneBricks", RenderType.terrain, Material.stone, 0, 1).setDisplayName("Stone Bricks");
        public static Tile TileGlass = new Tile("Glass", RenderType.alone, Material.stone, 1, 1).setTransparent().setWallBrightness(240);
        public static Tile TileTorch = new TileTorch("Torch", RenderType.placed, Material.furniture, 2, 1).setTransparent().setLight(200).notSolid().notWall().setWashedAwayByWater();
        public static Tile TileTree = new TileTree("Tree", RenderType.custom, Material.wood, 3, 1).setTransparent().notSolid();
        public static Tile TileLeaf = new TileLeaf("Leaf", RenderType.terrain, Material.wood, 4, 1, TileTree.index).setTransparent().notSolid();
        public static Tile TileWood = new Tile("Wood", RenderType.terrain, Material.wood, 4, 0).setDisplayName("Wooden Plank");
        public static Tile TileSand = new TileFalling("Sand", RenderType.terrain, Material.dirt, 5, 0);
        public static Tile TileSapling = new TileMustRestOn("Sapling", RenderType.custom, Material.furniture, 5, 1, 3, 3, 0, 1, TileGrass, true).notSolid().notWall().setTransparent().setWashedAwayByWater();
        public static Tile TileWoodDoor = new TileDoor("WoodDoor", RenderType.custom, 2, 2).notWall().setTransparent().setDisplayName("Wooden Door");
        public static Tile TileWoodTable = new TileFurniture("WoodTable", RenderType.custom, 3, 2, 3, 2, true).notSolid().notWall().setTransparent().setDisplayName("Wooden Table");
        public static Tile TileWoodChair = new TileFurniture("WoodChair", RenderType.custom, 4, 2, 1, 2, false).notSolid().notWall().setTransparent().setDisplayName("Wooden Chair");
        public static Tile TilePlantGlow = new TileMustRestOn("GlowLeaf", RenderType.custom, Material.furniture, 5, 1, 2, 3, 0, 1, new Tile[] { TileDirt, TileStone }, true).notSolid().notWall().setTransparent().setLight(120).setWashedAwayByWater().setDisplayName("Glow Leaf").setDisplayNamePlural("Glow Leaves");
        public static Tile TileLamp = new TileLightToggle("Lamp", RenderType.placed, Material.furniture, 6, 1).setTransparent().setLight(300).notSolid().notWall().setWashedAwayByWater();
        public static Tile TileWoodPlatform = new Tile("WoodPlatform", RenderType.platform, Material.wood, 5, 2).setTransparent().notSolid().notWall().setPlatform().setDisplayName("Wooden Platform");
        public static Tile TileLever = new TileLever("Lever", RenderType.placed, Material.furniture, 1, 3).setTransparent().notSolid();

        public int index;
        public string name;
        public RenderType renderType;
        public Material material;

        public bool transparent = false;
        private int light = 0;
        public Point textureArea;
        public int toughness = 255;
        public int wallBrightness = 127;
        public bool canBeWall = true;
        public bool washedAwayByWater = false;
        public string displayName;
        public string displayNamePlural;

        private static int nextIndex=0;

        private bool defaultSolid = true;
        private bool defaultPlatform = false;

        public Tile(string name, RenderType renderType, Material material, int textureX, int textureY) {
            index = getNewIndex();
            TileList[index] = this;
            this.name = name;
            this.material = material;
            this.displayName = name;
            this.displayNamePlural = displayName;
            this.renderType = renderType;
            this.textureArea = new Point(textureX, textureY);
        }

        private static int getNewIndex() {
            nextIndex++;
            return nextIndex-1;
        }

        private Tile notWall() {
            canBeWall = false;
            return this;
        }

        private Tile setThoughness(int t) {
            toughness = t;
            return this;
        }

        private Tile setWashedAwayByWater() {
            washedAwayByWater = true;
            return this;
        }

        private Tile setTransparent() {
            transparent = true;
            return this;
        }

        private Tile notSolid() {
            defaultSolid = false;
            return this;
        }

        private Tile setPlatform() {
            defaultPlatform = true;
            return this;
        }

        private Tile setLight(int power) {
            light = power;
            return this;
        }

        private Tile setWallBrightness(byte b) {
            wallBrightness = b;
            return this;
        }

        private Tile setDisplayName(string n) {
            displayName = n;
            displayNamePlural = n + "s";
            return this;
        }

        private Tile setDisplayNamePlural(string n) {
            displayNamePlural = n;
            return this;
        }


        public virtual TextureInfo getTextureInfo(int x, int y, World world, World.TileDepth tileDepth) {
            if (renderType == RenderType.terrain || renderType == RenderType.building || renderType == RenderType.alone) {
                
                bool left = false;
                bool right = false;
                bool down = false;
                bool up = false;

                if(renderType == RenderType.alone){
                    left = (world.getTileObject(x - 1, y, tileDepth)).index == index;
                    right = (world.getTileObject(x + 1, y, tileDepth)).index == index;
                    down = (world.getTileObject(x, y + 1, tileDepth)).index == index;
                    up = (world.getTileObject(x, y - 1, tileDepth)).index == index;
                } else {
                    left = (world.getTileObject(x - 1, y, tileDepth)).renderType == renderType;
                    right = (world.getTileObject(x + 1, y, tileDepth)).renderType == renderType;
                    down = (world.getTileObject(x, y + 1, tileDepth)).renderType == renderType;
                    up = (world.getTileObject(x, y - 1, tileDepth)).renderType == renderType;
                }

                Rectangle r;
                bool t = transparent;
                if(left){
                    if(right){
                        if(up){
                            if(down){
                                r = get8(1, 1);
                            } else {
                                r = get8(1, 2);
                                t = true;
                            }
                        } else {
                            if (down) {
                                r = get8(1, 0);
                                t = true;
                            } else {
                                r = get8(1, 3);
                                t = true;
                            }
                        }
                    } else {
                        if (up) {
                            if (down) {
                                r = get8(2, 1);
                                t = true;
                            } else {
                                r = get8(2, 2);
                                t = true;
                            }
                        } else {
                            if (down) {
                                r = get8(2, 0);
                                t = true;
                            } else {
                                r = get8(2, 3);
                                t = true;
                            }
                        }
                    }
                } else {
                    if (right) {
                        if (up) {
                            if (down) {
                                r = get8(0, 1);
                                t = true;
                            } else {
                                r = get8(0, 2);
                                t = true;
                            }
                        } else {
                            if (down) {
                                r = get8(0, 0);
                                t = true;
                            } else {
                                r = get8(0, 3);
                                t = true;
                            }
                        }
                    } else {
                        if (up) {
                            if (down) {
                                r = get8(3, 1);
                                t = true;
                            } else {
                                r = get8(3, 2);
                                t = true;
                            }
                        } else {
                            if (down) {
                                r = get8(3, 0);
                                t = true;
                            } else {
                                r = get8(3, 3);
                                t = true;
                            }
                        }
                    }
                }
                return new TextureInfo(/*texture2D,*/ r, t);
            } else if (renderType == RenderType.placed) {
                bool left = world.isTileSolid(x - 1, y, tileDepth);
                bool right = world.isTileSolid(x + 1, y, tileDepth);
                bool down = world.isTileSolid(x, y + 1, tileDepth);
                bool up = world.isTileSolid(x, y - 1, tileDepth);
                Rectangle r;
                if (down) {
                    r = get8(0, 0);
                } else if (left) {
                    r = get8(1, 0);
                } else if (right) {
                    r = get8(1, 1);
                } else if (up) {
                    r = get8(0, 1);
                } else {
                    r = get8(0, 0);
                }
                return new TextureInfo(/*texture2D,*/ r, true);
            } else if (renderType == RenderType.platform){
                bool leftS = world.isTileSolid(x - 1, y, tileDepth);
                bool rightS = world.isTileSolid(x + 1, y, tileDepth);
                bool leftP = world.getTileIndex(x - 1, y, tileDepth) == index;
                bool rightP = world.getTileIndex(x + 1, y, tileDepth) == index;
                int i = leftP ? 0 : leftS ? 1 : 2;
                int j = rightP ? 0 : rightS ? 1 : 2;
                return new TextureInfo(get8(i, j), true);
                /*
                	                left platform	left solid	left none
                    right platform	    -	            /-	        <-
                    right solid	        -\	            /\	        <\
                    right none	        ->	            />	        <>
                */
            }
            return new TextureInfo(/*Game.dummyTexture,*/ Game.OnexOneRect, true);
        }

        public Rectangle get8(int x, int y) {
            return new Rectangle((textureArea.X * 32) + (8 * x), (textureArea.Y * 32) + (8 * y), 8, 8);
        }

        public Rectangle get8(int x, int y, int w, int h) {
            return new Rectangle((textureArea.X * 32) + (8 * x), (textureArea.Y * 32) + (8 * y), 8*w, 8*h);
        }

        public static Tile getTileObject(int index) {
            if(index>=0 && index<TileList.Length){
                return TileList[index];
            }
            return null;
        }

        public virtual void updateRandom(World world, int x, int y, World.TileDepth tileDepth) {

        }

        public virtual void updateNearChange(World world, int x, int y, World.TileDepth tileDepth) {

        }

        public virtual ItemStack dropStack(World world, ItemPick itemPick, Random rand, int x, int y, World.TileDepth tileDepth) {
            return new ItemStack(Item.itemTile, 1, index);
        }

        public virtual void mine(World world, int x, int y, int data, ItemPick pick, World.TileDepth tileDepth) {

        }

        public bool canBeDestroyed(World world, int x, int y, World.TileDepth tileDepth) {
            if (world.getTileObject(x, y - 1, tileDepth).mustHaveTileBelow(world, x, y - 1, tileDepth)) {
                return false;
            }
            return true;
        }

        public virtual bool mustHaveTileBelow(World world, int x, int y, World.TileDepth tileDepth) {
            return false;
        }

        public bool canBePlacedHere(World world, int x, int y, World.TileDepth tileDepth) {
            if (tileDepth == World.TileDepth.wall && !canBeWall) {
                return false;
            }
            return canBePlacedHereOverridable(world, x, y, tileDepth);
        }

        public virtual bool canBePlacedHereOverridable(World world, int x, int y, World.TileDepth tileDepth) {
            if (renderType == RenderType.placed) {
                return (world.isTileSolid(x - 1, y, tileDepth) || world.isTileSolid(x + 1, y, tileDepth) || world.isTileSolid(x, y - 1, tileDepth) || world.isTileSolid(x, y + 1, tileDepth) || !world.getTileObject(x, y, tileDepth == World.TileDepth.wall ? World.TileDepth.tile : World.TileDepth.wall).isAir()) && world.getTileObject(x, y, tileDepth).isAir();
            }
            return (!world.getTileObject(x - 1, y, tileDepth).isAir() || !world.getTileObject(x + 1, y, tileDepth).isAir() || !world.getTileObject(x, y - 1, tileDepth).isAir() || !world.getTileObject(x, y + 1, tileDepth).isAir() || !world.getTileObject(x, y, tileDepth == World.TileDepth.wall ? World.TileDepth.tile : World.TileDepth.wall).isAir()) && world.getTileObject(x, y, tileDepth).isAir();
        }

        public bool isAir() {
            return (index == TileAir.index);
        }

        public virtual void justPlaced(World world, int x, int y, World.TileDepth tileDepth) {

        }

        public virtual bool drawHover(Game game, int mouseTileX, int mouseTileY, ItemStack currentItem) {
            return false;
        }

        public virtual ItemStack use(Game game, ItemStack currentItem, int mouseTileX, int mouseTileY, int mouseTileDistanceFromPlayer, World.TileDepth tileDepth) {
            return null;
        }

        public virtual bool isSolid(World world, int x, int y) {
            return defaultSolid;
        }

        public virtual bool isSolid() {
            return defaultSolid;
        }

        public virtual bool isPlatform(World world, int x, int y, World.TileDepth tileDepth) {
            return defaultPlatform;
        }

        public virtual Rectangle getItemRect(int data) {
            return get8(3, 3);
        }

        public virtual int getLight(int x, int y, World.TileDepth tileDepth) {
            return light;
        }

        public virtual float getDepth(int x, int y, World.TileDepth tileDepth) {
            return tileDepth == World.TileDepth.wall ? Game.RENDER_DEPTH_WALL : Game.RENDER_DEPTH_TILE;
        }
    }
}
