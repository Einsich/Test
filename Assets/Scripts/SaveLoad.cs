﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class SaveLoad : MonoBehaviour {

	public static void Save(byte SeaLevel, Texture2D[] landTex,Texture2D[] landNorm,byte[]terrain,byte[] HeightArr,int[,]provArr,List<Region> reg,List<State>st,string path)
    {
        using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
        {
            writer.Write(SeaLevel);
            int h = provArr.GetLength(0);
            int w = provArr.GetLength(1);
            writer.Write((byte)(h / MapMetrics.Tile));
            writer.Write((byte)(w / MapMetrics.Tile));
            for (int i = 0; i < 4; i++)
            {
                byte[] png = landTex[i].EncodeToPNG();
                writer.Write(png.Length);
                writer.Write(png);

                png = landNorm[i].EncodeToPNG();
                writer.Write(png.Length);
                writer.Write(png);
            }

            writer.Write(terrain);
            writer.Write(HeightArr);

            byte[] IdArr = new byte[h * w * 2];
            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                {
                    IdArr[(i * w + j) * 2] = (byte)(provArr[i, j] >> 8);
                    IdArr[(i * w + j) * 2 + 1] = (byte)(provArr[i, j]);
                }
            writer.Write(IdArr);

            writer.Write((short)reg.Count);
            for (int i = 0; i < reg.Count; i++)
            {
                writer.Write(reg[i].name);
                writer.Write((short)reg[i].Capital.x);
                writer.Write((short)reg[i].Capital.y);
                writer.Write(reg[i].iswater);
                writer.Write((short)reg[i].portIdto);

                writer.Write((byte)reg[i].neib.Length);
                foreach (Region x in reg[i].neib)
                    writer.Write((short)x.id);
            }
            writer.Write((short)st.Count);
            for (int i = 0; i < st.Count; i++)
            {
                //writer.Write((short)st[i].originalId);
                writer.Write((short)st[i].reg[0].id);
                writer.Write((byte)st[i].fraction);
                writer.Write((short)st[i].reg.Count);
                foreach (Region x in st[i].reg)
                    writer.Write((short)x.id);
            }

            for(int i=0;i<reg.Count;i++)
            {
                reg[i].data.Save(writer);
            }
        }
    }
    public static void Load(out byte SeaLevel,out Texture2D[] landTex,out Texture2D[] landNorm,out byte[]terrain,out byte[] HeightArr, out int[,] provArr, out List<Region> reg, out List<State> states,string path)
    {
        using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
        {
            SeaLevel = reader.ReadByte();
            int h = reader.ReadByte() * MapMetrics.Tile;
            int w = reader.ReadByte() * MapMetrics.Tile;

            landTex = new Texture2D[4];
            landNorm = new Texture2D[4];
            for (int i = 0; i < 4; i++)
            {
                landTex[i] = new Texture2D(w, h);
                int pngL = reader.ReadInt32();
                landTex[i].LoadImage(reader.ReadBytes(pngL));

                landNorm[i] = new Texture2D(w, h);
                pngL = reader.ReadInt32();
                landNorm[i].LoadImage(reader.ReadBytes(pngL));
            }
            terrain = reader.ReadBytes(w * h);

            HeightArr = reader.ReadBytes(h * w);

            byte[] topr = reader.ReadBytes(h * w * 2);
            provArr = new int[h, w];
            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                    provArr[i, j] = (topr[(i * w + j) * 2] << 8) + topr[(i * w + j) * 2 + 1];

            reg = new List<Region>();
            int regcount = reader.ReadInt16();
            for (int i = 0; i < regcount; i++)
                reg.Add(new Region());
            for (int i = 0; i < reg.Count; i++)
            {
                reg[i].id = i;
                reg[i].name = reader.ReadString();
                reg[i].Capital= new Vector2Int( reader.ReadInt16(), reader.ReadInt16());
                reg[i].iswater = reader.ReadBoolean();
                reg[i].portIdto = reader.ReadInt16();
                int l = reader.ReadByte();
                reg[i].neib = new Region[l];
                //reg[i].border = new List<GameObject>[l];
                //reg[i].arrow = new GameObject[l];
                for (int j = 0; j < l; j++)
                {
                    int k = reader.ReadInt16();
                    reg[i].neib[j] = reg[k];
                }
            }
            Texture2D colors = new Texture2D(32, 32);
            colors.LoadImage(File.ReadAllBytes( "Assets/Texture/Terrain/StateColor.png"));

            string[] names = File.ReadAllLines("Assets/Textes/States.txt");

            int stcount = reader.ReadInt16();
            states = new List<State>();
            for (int i = 0; i < stcount; i++)
                states.Add(new State());
            for (int i = 0; i < states.Count; i++)
            {
                int a0=reader.ReadInt16();
               // states[i].originalId = a0;
                if (i == 0) states[i].mainColor = new Color(0, 0, 0, 0);
                else
                states[i].mainColor = colors.GetPixel(a0 % 32, a0 / 32);
                states[i].name = names[a0];
                states[i].flag = new Texture2D(128, 128);
                states[i].flag.LoadImage(File.ReadAllBytes("Assets/Texture/flags/(" + a0 + ").png"));              

                states[i].Capital = reg[reader.ReadInt16()];
                states[i].fraction = (FractionName)reader.ReadByte();

                int l = reader.ReadInt16();
                for (int j = 0; j < l; j++)
                {
                    int k = reader.ReadInt16();
                    states[i].reg.Add(reg[k]);
                    reg[k].owner = states[i];
                }
            }

            for(int i=0;i<reg.Count;i++)
            {
                FractionName frac = reg[i].owner.fraction;
                reg[i].data = new ProvinceData(frac,reg[i]);
                reg[i].data.Load(reader);
            }

        }
    }
}
