using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TicTacToe.Models;

namespace TicTacToe.Controllers;

public class GameController : Controller
{
    private readonly IMemoryCache cache;

    // dependency-inject IMemoryCache
    public GameController(IMemoryCache cache)
    {
        this.cache = cache;
    }

    public IActionResult Index()
    {
        // pass current board for display in View
        ViewBag.board = (char[,])cache.Get("board");

        return View();
    }

    // assign players with player-ids. the first one to
    // register will be "player1", the second will be
    // "player2". once two players have registered, 
    // no more players can join.
    public IActionResult RegisterPlayer()
    {
        if (cache.Get("player1") == null) {
            cache.Set("player1", true);
			return Json(new {
                status = true,
                player_id = "player1",
                turn = "player1",
                board = BoardToString()
            });
		}

        if (cache.Get("player2") == null) {
            cache.Set("player2", true);
			return Json(new {
                status = true,
                player_id = "player2",
                turn = (string) cache.Get("turn"),
				board = BoardToString()
            });
		}

        // already have two players; can't accept anymore
		return Json(new { status = false });
	}

    
    public IActionResult AddPiece(string player, int row, int col)
    {
        char[,] board = (char[,]) cache.Get("board");

        bool status = false;

        if (player == (string) cache.Get("turn")
            && board[row, col] == ' ') {

            if (player == "player1") {
                // player 1 will use 'X'
                board[row, col] = 'X';
                cache.Set("board", board);
                cache.Set("turn", "player2");
                status = true;
            }
            else if (player == "player2") {
                // player 2 will use 'O'
                board[row, col] = 'O';
                cache.Set("board", board);
                cache.Set("turn", "player1");
                status = true;
            }
        }

		return Json(new {
            status = status,
            turn = (string) cache.Get("turn"),
            board = BoardToString()
        });

	}

    // return current states of game
    public IActionResult Ping()
    {
        return Json(new {
            turn = (string) cache.Get("turn"),
            board = BoardToString()
        });
    }

    // char[,] cannot be converted into a JSON value,
    // hence, we need to explicitly break it into a
    // semi-colon delimited string.
    private string BoardToString()
    {
        char[,] board = (char[,])cache.Get("board");

        string str = "";

        for (int i=0; i<3; i++) {
            for (int j=0; j<3; j++) {
                str += board[i, j] + ";";
            }
        }

        // remove the final ";"
        return str.Substring(0, str.Length-1);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}