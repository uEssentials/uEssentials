"""

  This file is part of uEssentials project.
      https://uessentials.github.io/

  Copyright (C) 2015-2016  Leonardosc

  This program is free software; you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation; either version 2 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License along
  with this program; if not, write to the Free Software Foundation, Inc.,
  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

"""

import sys, os, shutil

# Rocket Plugins Folder
PLUGINS_FOLDER = "C:\\Users\\Leonardo\\Documents\\Unturned\\Rocket\\All\\Unturned\\Servers\\MyFirstRocketServer\Rocket\\Plugins\\"

def build(args):
    print("Build() %s"%args)
    if len(args) == 0:
        # Development build
        os.system("msbuild /nologo")
    else:
        build_type = args[0]
        # Release build
        if build_type == "RELEASE":
            os.system("msbuild /p:DefineConstants="" /t:Rebuild,Clean")
        elif build_type == "EXPERIMENTAL":
            # Check if has EXPERIMENTAL_HASH
            if len(args) > 2 and args[1] == "EXPERIMENTAL_HASH":
                # Replace COMMIT_HASH
                copy_file("src\\Core\\EssCore.cs", "src\\Core\\EssCore.cs.old")
                file_reader = open("src\\Core\\EssCore.cs.old", 'r')
                file_writer = open("src\\Core\\EssCore.cs", 'w')

                for line in file_reader.readlines():
                    if "$COMMIT_HASH$" in line:
                        print("Replaced $COMMIT_HASH$ in %s"%line)
                        line = line.replace("$COMMIT_HASH$", args[2])
                    file_writer.write(line)

                file_reader.close()
                file_writer.close()

                os.system("msbuild /p:DefineConstants=\"EXPERIMENTAL;EXPERIMENTAL_HASH\" /t:Rebuild,Clean")
                os.remove("src\\Core\\EssCore.cs")
                os.rename("src\\Core\\EssCore.cs.old", "src\\Core\\EssCore.cs")
            else:
                os.system("msbuild /p:DefineConstants=\"EXPERIMENTAL\" /t:Rebuild,Clean")
        else:
            print("Invalid build type: %s"%build_type)

def copy_to_plugins():
    shutil.copy2("%s\\bin\\uEssentials.dll"%sys.path[0], "%suEssentials.dll"%PLUGINS_FOLDER)

if __name__ == '__main__':
    if len(sys.argv) == 1:
        print("Invalid options...")
        sys.exit(1)

    args = sys.argv;
    args.pop(0)
    option = args[0]

    if option == "BUILD":
        args.pop(0) # Remove 'BUILD'
        build(args)
    elif option == "COPY_TO_PLUGINS":
        copy_to_plugins()
    else:
        print("Invalid option %s"%option)

    sys.exit(0)
