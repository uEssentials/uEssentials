#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2018  leonardosnt
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/
#endregion

using Essentials.Common;
using System;
using System.Linq;

namespace Essentials.Api.Unturned {

    public class USkill {

        public static readonly USkill OVERKILL = new USkill(0, 0, "OVERKILL", 7);
        public static readonly USkill SHARPSHOOTER = new USkill(0, 1, "SHARPSHOOTER", 7);
        public static readonly USkill DEXTERITY = new USkill(0, 2, "DEXTERITY", 5);
        public static readonly USkill CARDIO = new USkill(0, 3, "CARDIO", 5);
        public static readonly USkill EXERCISE = new USkill(0, 4, "EXERCISE", 5);
        public static readonly USkill DIVING = new USkill(0, 5, "DIVING", 5);
        public static readonly USkill PARKOUR = new USkill(0, 6, "PARKOUR", 5);
        public static readonly USkill SNEAKYBEAKY = new USkill(1, 0, "SNEAKYBEAKY", 7);
        public static readonly USkill VITALITY = new USkill(1, 1, "VITALITY", 5);
        public static readonly USkill IMMUNITY = new USkill(1, 2, "IMMUNITY", 5);
        public static readonly USkill TOUGHNESS = new USkill(1, 3, "TOUGHNESS", 5);
        public static readonly USkill STRENGTH = new USkill(1, 4, "STRENGTH", 5);
        public static readonly USkill WARMBLOODED = new USkill(1, 5, "WARMBLOODED", 5);
        public static readonly USkill SURVIVAL = new USkill(1, 6, "SURVIVAL", 5);
        public static readonly USkill HEALING = new USkill(2, 0, "HEALING", 7);
        public static readonly USkill CRAFTING = new USkill(2, 1, "CRAFTING", 3);
        public static readonly USkill OUTDOORS = new USkill(2, 2, "OUTDOORS", 5);
        public static readonly USkill COOKING = new USkill(2, 3, "COOKING", 3);
        public static readonly USkill FISHING = new USkill(2, 4, "FISHING", 5);
        public static readonly USkill AGRICULTURE = new USkill(2, 5, "AGRICULTURE", 7);
        public static readonly USkill MECHANIC = new USkill(2, 6, "MECHANIC", 5);
        public static readonly USkill ENGINEER = new USkill(2, 7, "ENGINEER", 3);

        public static readonly USkill[] Skills = {
            OVERKILL,
            SHARPSHOOTER,
            DEXTERITY,
            CARDIO,
            EXERCISE,
            DIVING,
            PARKOUR,
            SNEAKYBEAKY,
            VITALITY,
            IMMUNITY,
            TOUGHNESS,
            STRENGTH,
            WARMBLOODED,
            SURVIVAL,
            HEALING,
            CRAFTING,
            OUTDOORS,
            COOKING,
            FISHING,
            AGRICULTURE,
            MECHANIC,
            ENGINEER
        };

        internal byte SpecialityIndex;
        internal byte SkillIndex;
        internal byte Max;

        public string Name { get; }

        private USkill(byte specialityIndex, byte skillIndex, string name, byte max)
        {
            SpecialityIndex = specialityIndex;
            SkillIndex = skillIndex;
            Name = name;
            Max = max;
        }

        /// <summary>
        ///   Get skill from name.
        /// </summary>
        /// <param name="name">Skill name</param>
        /// <returns>
        ///   <see cref="Optional{USkill}.Empty"/> if not found,
        ///   otherwise return a <see cref="Optional{USkill}"/> containing the skill.
        /// </returns>
        [Obsolete("Use FromName(string name, out USkill skill) instead.")]
        public static Optional<USkill> FromName(string name) {
            var skill = Skills.FirstOrDefault(sk => sk.Name.EqualsIgnoreCase(name)) ??
                        Skills.FirstOrDefault(sk => sk.Name.ContainsIgnoreCase(name));
            return Optional<USkill>.OfNullable(skill);
        }

        public static bool FromName(string name, out USkill skill) {
            skill = Skills.FirstOrDefault(sk => sk.Name.EqualsIgnoreCase(name)) ??
                    Skills.FirstOrDefault(sk => sk.Name.ContainsIgnoreCase(name));
            return skill != null;
        }
    }

}