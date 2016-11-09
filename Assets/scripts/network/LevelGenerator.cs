using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Assets.scripts.network
{

    class LevelGenerator: MonoBehaviour
    {


        [Header("Stairs")]
        public GameObject stairCaseFirst;
        public GameObject stairCase;
        public GameObject stairCaseLast;

        [Header("Rooms")]
        public GameObject[] rooms;

        [Header("Params")]
        public int floors;
        public int nbMinStair = 1;
        public int nbMaxStair = 1;
        public int size = 5;
        public Vector3 blockSize = Vector3.one;

        private int levelSizeX;
        private int levelSizeY;


        private static float getRandom(float min, float max)
        {
            return UnityEngine.Random.value * (max - min) + min;
        }

        private bool isFree(ref int[][] grid, int x, int y, int offX, int offY)
        {
            for (var j = y; j < y + offY; ++j)
            {
                if (j >= grid.Length) return false;
                for (var i = x; i < x + offX; ++i)
                {
                    if (i >= grid[y].Length) return false;
                    if (grid[j][i] != 0) return false;
                }
            }
            return true;
        }


        private void fillGrid(ref int[][] grid, int x, int y, int offX, int offY, int type)
        {
            int p = 0;
            for (var j = 0; j < offY; ++j)
            {
                for (var i = 0; i < offX; ++i)
                {
                    grid[j+ y][i + y] = type + p;
                    if (i == 0 && j == 0) p = 1; //upper left corner parent, others are child => lower bit set to 1
                }
            }
        }


        private void initGrid(ref int[][] grid, int sizeX, int sizeY, ref Vector2[] stairs) 
        {
            grid = new int[sizeY][];
            for (var y = 0; y < sizeY; ++y) 
            {
                grid[y] = new int[sizeX];
                for (var x = 0; x < sizeX; ++x)
                {
                    grid[y][x] = 0;
                    if (stairs != null)
                    {
                        if (stairs.Contains(new Vector2(x,y)))
                        {
                            fillGrid(ref grid, x, y, 2, 2, 10);
                        }
                    }
                }
            }
        }

        private void buildLevel(ref int[][] grid, int floor, int nbFloors)
        {
           for (var y = 0; y < levelSizeY; ++y) {
                for (var x = 0; x < levelSizeX; ++x) 
                {
                    Vector3 v = new Vector3(x * blockSize.x, y * blockSize.y, floor * blockSize.z);
                    if (grid[y][x] == 10) // stairCase
                    {
                        if (floor == 0)
                        {
                            Instantiate(stairCaseFirst, v, Quaternion.identity);
                        } 
                        else if (floor == nbFloors) 
                        {
                            Instantiate(stairCaseLast, v, Quaternion.identity);
                        }
                        else 
                        {
                            Instantiate(stairCase, v, Quaternion.identity);
                        }
                    }
                }
            }
        }



        public void Start()
        {
            Debug.Log("Start procedural level generation");
            levelSizeX = 10;
            levelSizeY = 5;
            int[][] grid = null;
            Vector2[] stairsPosition = null;
            initGrid(ref grid, levelSizeX, levelSizeY, ref stairsPosition);
            var nbStairs = (int)getRandom(nbMinStair, nbMaxStair);
            stairsPosition = new Vector2[nbStairs];
            for (var s = 0; s < nbStairs; ++s)
            {
                bool added = false;
                do
                {
                    var x = (int)getRandom(0, levelSizeX);
                    var y = (int)getRandom(0, levelSizeY);
                    if (isFree(ref grid, x, y, 2, 2))
                    {
                        added = true;
                        fillGrid(ref grid, x, y, 2, 2, 10);
                        stairsPosition[s] = new Vector2(x, y);
                    }
                } while (!added);

            }

            //last step, build level floor
            buildLevel(ref grid, 0, 10);

        }






    }
}
