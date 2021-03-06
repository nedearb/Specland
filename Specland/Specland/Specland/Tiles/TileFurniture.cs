﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Specland {
    public class TileFurniture : Tile {

        public Point size = new Point(0, 0);
        private bool topIsPlatform = false;

        public TileFurniture(string name, RenderType renderType, int textureX, int textureY, int width, int height, bool platform)
            : base(name, renderType, Material.furniture, textureX, textureY) {
            size = new Point(width, height);
            topIsPlatform = platform;
        }

        public override TextureInfo getTextureInfo(int x, int y, World world, World.TileDepth tileDepth) {

            Rectangle r = get8(3, 3);

            int l = world.getTileDataNoCheck(x, y, tileDepth);

            int k = 1;

            bool kk = false;

            for (int i = 0; i < size.X; i++) {
                for (int j = size.Y - 1; j >= 0; j--) {
                    if (k == l || k == -l) {
                        r = get8(l<0?(size.X-1-i):i, j);
                        kk = true;
                        break;
                    }
                    k++;
                }
                if (kk) {
                    break;
                }
            }


            return new TextureInfo(r, true, Point.Zero, l<0, false);
        }

        public override void mine(World world, int x, int y, int data, ItemPick pick, World.TileDepth tileDepth) {

            int k = 1;

            int ii = 0;
            int jj = 0;
            bool kk = false;

            for (int i = 0; i < size.X; i++) {
                for (int j = size.Y-1; j >=0 ; j--) {
                    if (k == data || k == -data) {
                        ii = i;
                        jj = j;
                        kk = true;
                        break;
                    }
                    k++;
                }
                if (kk) {
                    break;
                }
            }

            for (int i = 0; i < size.X; i++) {
                for (int j = size.Y - 1; j >= 0; j--) {
                    world.setTileWithUpdate(x + i - ii, y + j - jj, Tile.TileAir.index, tileDepth);
                }
            }

        }

        public override void updateNearChange(World world, int x, int y, World.TileDepth tileDepth) {
            if (!(world.isTileSolid(x, y + 1, tileDepth) || world.getTileIndex(x, y + 1, tileDepth) == index)) {
                world.mineTile(x, y, Item.itemSupick, tileDepth);
            }
        }

        public override bool canBePlacedHereOverridable(World world, int x, int y, World.TileDepth tileDepth) {

            for (int i = 0; i < size.X; i++) {
                for (int j = size.Y - 1; j >= 0; j--) {

                    if (!world.getTileObject(x + i, y + j - (size.Y - 1), tileDepth).isAir()) {
                        return false;
                    }
                }
            }

            for (int i = 0; i < size.X; i++) {
                if (!world.isTileSolid(x + i, y + 1, tileDepth)) {
                    return false;
                }
            }

            return true;
        }

        public override void justPlaced(World world, int x, int y, World.TileDepth tileDepth) {

            bool right = Game.instance.currentWorld.player.facingRight;

            int k = 1;

            for (int i = 0; i < size.X; i++) {
                for (int j = size.Y - 1; j >= 0; j--) {
                    world.setTileWithDataWithUpdate(x + i, y + j-(size.Y-1), index, right?k:-k, tileDepth);
                    k++;
                }
            }

        }

        public override ItemStack dropStack(World world, ItemPick itemPick, Random rand, int x, int y, World.TileDepth tileDepth) {
            return new ItemStack(Item.itemTile, 1, index);
        }

        public override bool drawHover(Game game, int mouseTileX, int mouseTileY, ItemStack currentItem) {
            if (canBePlacedHere(game.currentWorld, mouseTileX, mouseTileY, World.TileDepth.tile)) {
                Rectangle rect = get8(0, 0);
                rect.Width = 8 * size.X;
                rect.Height = 8 * size.Y;
                game.spriteBatch.Draw(Tile.TileSheet, new Rectangle(((mouseTileX) * World.tileSizeInPixels) - game.currentWorld.viewOffset.X, ((mouseTileY - (size.Y - 1)) * World.tileSizeInPixels) - game.currentWorld.viewOffset.Y, World.tileSizeInPixels * size.X, World.tileSizeInPixels * size.Y), rect, new Color(.5f, .5f, .5f, .5f), 0, Vector2.Zero, game.currentWorld.player.facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally, Game.RENDER_DEPTH_HOVER);
            }
            return true;
        }

        private void updateFurniture(Game game, int x, int y) {

            for (int i = -size.X; i < size.X; i++) {
                for (int j = -size.Y; j < size.Y; j++) {
                    game.currentWorld.calculateTileFrame(game, x + i, y + j, World.TileDepth.tile);
                }
            }

        }

        public override bool isSolid(World world, int x, int y) {
            return base.isSolid(world, x, y);
        }

        public override bool isPlatform(World world, int x, int y, World.TileDepth tileDepth) {
            return topIsPlatform && Math.Abs(world.getTileData(x, y, tileDepth))%size.Y==0;
        }
    }
}
