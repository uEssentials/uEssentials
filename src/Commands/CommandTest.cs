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

using System;
using System.Reflection;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using SDG.Unturned;
using UnityEngine;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "test"
    )]
    public class CommandTest : EssCommand
    { 
        public override void OnExecute( ICommandSource source, ICommandArgs parameters )
        {
            var player = source.ToPlayer();
            var p = player.UnturnedPlayer;

          //  Console.WriteLine( p.GetComponent<CharacterController>() == null );
            //   p.movement.GetComponent<CharacterController>().attachedRigidbody.constraints = RigidbodyConstraints.FreezePositionX;



            return;

            RaycastHit hit;

            Physics.Raycast(p.look.aim.position, p.look.aim.forward, out hit, 114f, RayMasks.PLAYER_INTERACT);

            
            var barricade = hit.transform?.GetComponent<InteractableDoor>();

            byte b;
			byte b2;
			ushort num;
			ushort num2;
			BarricadeRegion barricadeRegion;
            if ( BarricadeManager.tryGetInfo( barricade.transform, out b, out b2, out num, out num2, out barricadeRegion ) )
            {
                Console.WriteLine( b + " " +b2 + " " + num + " " + num2) ;
            }


            return;
            var animalManager = typeof( AnimalManager ).GetField( "manager", BindingFlags.NonPublic | BindingFlags.Static ).GetValue( null ) as AnimalManager;
            
            typeof( AnimalManager ).GetMethod( "addAnimal", BindingFlags.NonPublic | BindingFlags.Instance )
            .Invoke( animalManager, new object[] { (byte) 3, player.Position + new Vector3( 2, 0, 0 ), 0.0f, false } );
            
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
