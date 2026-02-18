using Sprout;
using Sprout.Tests.TestGame;

AppInfo info = new AppInfo("Test Game");
using Game game = new Game();
game.Run(in info);