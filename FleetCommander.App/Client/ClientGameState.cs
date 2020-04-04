using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetCommander.App.Shared;
using Microsoft.JSInterop;

namespace FleetCommander.App.Client
{
    public static class ClientGameState
    {
        private static GameBoard _gameBoard;
        private static ILookup<string, IToken> _tokenLookup;

        public static GameBoard GameBoard
        {
            get => _gameBoard;
            set
            {
                _gameBoard = value;
                _tokenLookup = _gameBoard.CreateTokenLookup();
            }
        }

        [JSInvokable("ClientGameState.GetTokens")]
        public static async Task<IToken[]> GetTokens(int row, int col)
        {
            if (_tokenLookup == null)
            {
                return new ShipToken[0];
            }
            else
            {
                return _tokenLookup[row + "," + col].ToArray();
            }

        }
    }
}
