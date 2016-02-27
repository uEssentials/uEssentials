/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2016  Leonardosc
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

using System.Reflection;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Common;
using SDG.Unturned;
using UnityEngine;
using System.Text;
using Essentials.InternalModules.Kit.Commands;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "test"
     )]
    public class CommandTest : EssCommand
    {
        public static string tostr( byte[] obj )
        {
            var sb = new StringBuilder();
            obj.ForEach( o => sb.Append( o.ToString() ).Append( ", " ) );
            return sb.Remove( 0, sb.Length ).Append( "]" ).ToString();
        }


        public override void OnExecute( ICommandSource src, ICommandArgs args )
        {
            var player = src.ToPlayer();
            var _equip = player.Equipment;
            //
            
            src.SendMessage( CommandKit.Cooldowns[player.CSteamId.m_SteamID].Count );



























































            return;
            var p = player.UnturnedPlayer;


            RaycastHit hit;

            Physics.Raycast( p.look.aim.position, p.look.aim.forward, out hit, 114f, RayMasks.BARRICADE | RayMasks.STRUCTURE );


            var a = hit.transform?.GetComponent<Interactable>();
            //    var b = hit.transform?.GetComponent<Interactable2>();

            //  BarricadeManager.toggleDoor( hit.transform );
            //   BarricadeManager.toggleDoor( hit.transform.GetChild( 0 ) );
            // BarricadeManager.damage( a.transform, float.MaxValue, 1 );


            return;
            var animalManager = typeof (AnimalManager).GetField( "manager", BindingFlags.NonPublic | BindingFlags.Static ).GetValue( null ) as AnimalManager;

            typeof (AnimalManager).GetMethod( "addAnimal", BindingFlags.NonPublic | BindingFlags.Instance )
                .Invoke( animalManager, new object[] {(byte) 3, player.Position + new Vector3( 2, 0, 0 ), 0.0f, false} );

            animalManager.askAnimals( player.CSteamId );

            return;


            //var animalManager = typeof( AnimalManager ).GetField( "manager", BindingFlags.NonPublic | BindingFlags.Static ).GetValue( null ) as AnimalManager;
            //animalManager.askAnimals( player.CSteamId );
            //var animals = typeof( AnimalManager ).GetField( "_animals", BindingFlags.NonPublic | BindingFlags.Static ).GetValue( null ) as List<Animal>;

            //typeof( AnimalManager ).GetMethod( "addAnimal", BindingFlags.NonPublic | BindingFlags.Instance )
            //.Invoke( animalManager, new object[] { (byte) 3, player.GetPosition() + new Vector3( 2, 0, 0 ), 0.0f, false } );

            //animalManager.channel.openWrite();
            //animalManager.channel.write( (ushort) AnimalManager.animals.Count );
            //Animal animal = AnimalManager.animals[AnimalManager.animals.Count - 1];
            //animalManager.channel.write( new object[]
            //{
            //        animal.id,
            //        animal.transform.position,
            //        MeasurementTool.angleToByte(animal.transform.rotation.eulerAngles.y),
            //        animal.isDead
            //} );
            //     animalManager.channel.closeWrite( "tellAnimals", player.GetCSteamId, ESteamPacket.UPDATE_RELIABLE_CHUNK_BUFFER );

            //   animalManager.askAnimals(player.GetCSteamId);

            //  source.SendMessage();


            return;
            //  var barel = new Attachment(7, 100);
            //   var grip = new Attachment(8, 100);
            //   var sight = new Attachment(146, 100);

            //     UnturnedItems.AssembleItem(4,20, sight, null, grip, barel, new Attachment(17, 100));
        }

        //player.GetUnturnedPlayer().Inventory.Items[(int)PlayerInventory.BACKPACK].resize((byte)20, (byte)20);
    }
}