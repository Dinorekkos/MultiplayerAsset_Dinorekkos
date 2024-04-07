using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dino.MultiplayerAsset
{
    /// <summary>
    /// Random Names just for fun, name given based on their anonymous ID
    /// </summary>
    public static class NameGenerator
    {
        public static string GetRandomName(string userId)
        {
            int seedId = userId.GetHashCode();
            seedId *= Math.Sign(seedId);
            StringBuilder nameOutput = new StringBuilder();
            
            int word = seedId % 22;
            if (word == 0) // Note that some more data-driven approach would be better.
                nameOutput.Append("Ant");
            else if (word == 1)
                nameOutput.Append("Bear");
            else if (word == 2)
                nameOutput.Append("Crow");
            else if (word == 3)
                nameOutput.Append("Dog");
            else if (word == 4)
                nameOutput.Append("Eel");
            else if (word == 5)
                nameOutput.Append("Frog");
            else if (word == 6)
                nameOutput.Append("Gopher");
            else if (word == 7)
                nameOutput.Append("Heron");
            else if (word == 8)
                nameOutput.Append("Ibex");
            else if (word == 9)
                nameOutput.Append("Jerboa");
            else if (word == 10)
                nameOutput.Append("Koala");
            else if (word == 11)
                nameOutput.Append("Llama");
            else if (word == 12)
                nameOutput.Append("Moth");
            else if (word == 13)
                nameOutput.Append("Newt");
            else if (word == 14)
                nameOutput.Append("Owl");
            else if (word == 15)
                nameOutput.Append("Puffin");
            else if (word == 16)
                nameOutput.Append("Rabbit");
            else if (word == 17)
                nameOutput.Append("Snake");
            else if (word == 18)
                nameOutput.Append("Trout");
            else if (word == 19)
                nameOutput.Append("Vulture");
            else if (word == 20)
                nameOutput.Append("Wolf");
            else
                nameOutput.Append("Zebra");
        

            int number = seedId % 1000;
            nameOutput.Append(number.ToString("000"));

            return nameOutput.ToString();
            
        }
    }

}