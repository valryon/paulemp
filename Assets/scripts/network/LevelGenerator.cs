using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Assets.scripts.network
{

    class LevelGenerator
    {
        [Header("Stairs")]
        public GameObject stairCaseFirst;
        public GameObject stairCase;
        public GameObject stairCaseLast;

        [Header("")]
        public GameObject[] rooms;

        [Header("Params")]
        public Vector2 floors;
        public int nbMinStair = 1;
        public int nbMaxStair = 1;
        public int size = 5;


        private static float getRandom(float min, float max)
        {
            return UnityEngine.Random.value * (max - min) + min;
        }


        private void fillGrid(ref int[][] grid, int x, int y, int offX, int offY, int type)
        {
            for (var j = y; j < y + offY; ++j)
            {
                for (var i = x; i < x + offX; ++i)
                {
                    grid[j][i] = type;
                }
            }
        }


        private void initGrid(ref int[][] grid, int sizeX, int sizeY) {
            grid = new int[sizeY][];
            for (var y = 0; y < sizeY; ++y) {
                grid[y] = new int[sizeX];
                for (var x = 0; x < sizeX; ++x) {
                    grid[y][x] = 0;
                }
            }
        }





        public void generate()
        {
            int[][] grid = null;
            initGrid(ref grid, 5, 10);
            var nbStairs = getRandom(nbMinStair, nbMaxStair);
            

        }



    }
}
