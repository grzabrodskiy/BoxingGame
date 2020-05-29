﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Main
{
    //Holds all the games fighters
    public class FighterCache
    {
        public FighterCache()
        {
        }

        //all fighters
        private ConcurrentDictionary<string, Fighter> Cache = new ConcurrentDictionary<string, Fighter>();

        public Fighter Get(string name)
        {
            return Cache[name];
        }

        public bool TryAdd(Fighter fighter)
        {
            return Cache.TryAdd(fighter.Name, fighter);
        }

        public List<Fighter> AllFighters()
        {
            return new List<Fighter>(Cache.Values);
        }

        public int Count()
        {
            return Cache.Count;
        }

        public Fighter CreateRandomFighter(int weight = -1)
        {
            Fighter hungryYoungLion = NewFighterWithRandomName();

            string[] normalCombatTraits =
            ConfigurationManager.AppSettings["ScaledFighterCombatTraits"].Split(",");

            if (weight == -1)
                weight = AssignWeightClass().Weight;

            hungryYoungLion.Weight = weight;

            int totalSkillPoints = RandomSkillLevel() * normalCombatTraits.Count();

            foreach(string property in normalCombatTraits)
                hungryYoungLion.GetType().GetProperty(property).SetValue(hungryYoungLion, 0, null);

            while (totalSkillPoints > 0)
            {
                int toUpgrade = MathUtils.RangeUniform(0, normalCombatTraits.Count());
                var property = hungryYoungLion.GetType().GetProperty(normalCombatTraits[toUpgrade]);
                int currentSkill = (int) property.GetValue(hungryYoungLion);
                if (currentSkill < 100)
                {
                    property.SetValue(hungryYoungLion, currentSkill + 1, null);
                    --totalSkillPoints;
                }
            }

            return hungryYoungLion;
        }

        public Fighter NewFighter(string name, bool force = false)
        {
            Fighter fighter = new Fighter(name);

            if (force)
            {
                Cache[name] = fighter;
                return fighter;
            }

            return Cache.TryAdd(fighter.Name, fighter) ? fighter : null;
        }

        //Assign the fighter a unique name - must be unique this will
        //be the fighter's ID!! 
        public Fighter NewFighterWithRandomName()
        {
            Fighter fighter = null;

            for(int attempt = 0; attempt < 10 && fighter == null; ++attempt)
            {
                string name = Utility.getRandomFirstName() + " " + Utility.getRandomLastName();
                fighter = NewFighter(name);
            }
            if (fighter == null)
                throw new InvalidOperationException("Fighter Unique Name Generation Failed");

            return fighter;
        }

        //todo: should be in config somewhere
        readonly static (int hi, double numFighters)[] SKILL_DISTRIBUTION = new (int hi, double numFighters)[]{
            (19, 20328),
            (49, 60985.5),
            (59, 6800),
            (66, 3400),
            (71, 1020),
            (76, 680),
            (81, 340),
            (86, 160),
            (91, 120),
            (94, 28),
            (97, 14),
            (99, 4),
            (100,1),
         };

        static double TOTAL_FIGHTERS_DISTRO = SKILL_DISTRIBUTION.Select(element => element.numFighters).Sum();


        //According to the expected skill distribution 
        private static int RandomSkillLevel()
        {
            double target = MathUtils.RangeUniform(0, TOTAL_FIGHTERS_DISTRO);
            double sum = 0;
            int lo = -1;
            int hi = -1;
            for(int i = 0; sum < target; ++i)
            {
                lo = hi + 1;
                hi = SKILL_DISTRIBUTION[i].hi;
                sum += SKILL_DISTRIBUTION[i].numFighters;
            }

            return MathUtils.RangeUniform(lo, hi); 
        }

        //According to the expected size of the weight class
        private static WeightClass AssignWeightClass()
        {
            List<WeightClass> weights = WeightClass.AllWeightClasses();
            int sizeSum = WeightClass.WC_SIZE_SUM;

            double target = MathUtils.RangeUniform(0, sizeSum);
            double sum = -1;
            int w = -1;

            do
            {
                sum += weights[++w].Size;
            }
            while (sum < target);

            return weights[w];
        }

 


    }
}
