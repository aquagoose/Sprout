using Sprout;
using Sprout.Tests.TestGame;

AppInfo info = new AppInfo("Test Game", "1.0.0");
using Game game = new Game();
game.Run(in info);