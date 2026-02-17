using System;
using System.Numerics;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== THE BRUTAL ORACLE: CLUE QUALITY EDITION ===");
        Console.WriteLine("You have 5 guesses to divine the Master Number.");
        Console.WriteLine("Clues must be EARNED.\n");

        Console.Write("Enter your seeker name: ");
        string name = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(name)) name = "Seeker";

        const int MasterNumber = 42;
        const int MaxTurns = 5;

        // Ritual layer (secret): base^exponent
        Random rng = new Random();
        int baseNumber = rng.Next(2, 21);   // keep ritual somewhat sane: 2..20
        int exponent = 1;

        // Track clues already given so we don't repeat
        HashSet<string> usedClues = new HashSet<string>();

        Console.WriteLine($"\nWelcome, {name}.");
        Console.WriteLine("The Oracle watches your numbers, not your intentions.");
        Console.WriteLine("Win condition: speak the Master Number exactly.\n");

        for (int turn = 1; turn <= MaxTurns; turn++)
        {
            Console.WriteLine($"Turn {turn}/{MaxTurns}  |  Ritual Exponent: {exponent}");
            Console.Write("Speak a number: ");

            string input = Console.ReadLine()?.Trim() ?? "";
            if (!BigInteger.TryParse(input, out BigInteger guess))
            {
                Console.WriteLine("The Oracle does not understand that shape. (Enter a whole number.)\n");
                turn--; // don't waste a turn
                continue;
            }

            // Win instantly on master
            if (guess == MasterNumber)
            {
                WinReveal(name);
                return;
            }

            // Ritual evaluation
            BigInteger currentTarget = BigInteger.Pow(baseNumber, exponent);
            BigInteger tolerance = GetGlidingTolerance(currentTarget, exponent);
            BigInteger diff = BigInteger.Abs(guess - currentTarget);

            bool earnedClue = (exponent > 1) && (diff <= tolerance);

            if (earnedClue)
            {
                exponent--;
                Console.WriteLine("The air stills. The ritual slackens.");
                Console.WriteLine($"(You were close enough: |guess - target| = {diff} ≤ {tolerance})");

                string clue = GenerateNuancedClue(MasterNumber, exponent, usedClues, rng);
                if (!string.IsNullOrWhiteSpace(clue))
                {
                    Console.WriteLine($"CLUE: {clue}\n");
                }
                else
                {
                    Console.WriteLine("CLUE: The Oracle withholds repetition.\n");
                }
            }
            else
            {
                exponent++;
                Console.WriteLine("The pressure increases. The ritual intensifies.");
                Console.WriteLine($"(Not close enough: |guess - target| = {diff} > {tolerance})\n");
            }

            if (exponent < 1) exponent = 1;
            if (exponent > 12) exponent = 12;
        }

        Console.WriteLine("The Oracle closes its eye.");
        Console.WriteLine("You leave with fragments, not truth.\n");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    // Tolerance glides with target magnitude and exponent.
    static BigInteger GetGlidingTolerance(BigInteger target, int exponent)
    {
        // Slightly forgiving because clues are gated behind closeness.
        // e1: 6%, e2: 9%, e3: 12%, ... capped at 30%
        int percent = Math.Min(30, 6 + (exponent - 1) * 3);

        BigInteger tol = (target * percent) / 100;

        // Minimum so the ritual isn't impossible early.
        if (tol < 15) tol = 15;

        return tol;
    }

    static string GenerateNuancedClue(int master, int currentExponentAfterDrop, HashSet<string> used, Random rng)
    {
        // Lower exponent => better clue quality, but still not a direct giveaway.
        // We'll pull from a pool of "clue families" and avoid repeats.

        List<string> candidates = new List<string>();

        // ---- Oblique / vibe clues (high exponent, low quality) ----
        // Still true, but more poetic than diagnostic.
        candidates.Add("Some answers are famous not because they're correct, but because everyone keeps asking the wrong question.");

        // ---- Light math constraints (mid quality) ----
        candidates.Add("It is even, but it refuses to be a tidy square.");
        candidates.Add("It is not prime, yet it isn't crowded with factors.");
        candidates.Add("Its prime structure is small: only two primes share custody.");

        // ---- Modular / residue hints (stronger, but not explicit) ----
        // (These are surprisingly helpful without giving away digits.)
        int mod5 = master % 5;
        int mod8 = master % 8;
        int mod9 = master % 9;

        candidates.Add($"When divided by 5, it leaves a remainder of {mod5}.");
        candidates.Add($"When divided by 8, it leaves a remainder of {mod8}.");
        candidates.Add($"Its digit-sum leaves remainder {mod9} when divided by 9.");

        // ---- Range-ish hints (strongest, but still not “contains 4 and 2”) ----
        // Keep these subtle: relative to landmarks, not explicit bounds.
        candidates.Add("It sits closer to 50 than to 30.");
        candidates.Add("It lives in the low forties—near a familiar hitchhiker’s whisper.");
        candidates.Add("If you walk up from 40, you won't need more than one hand.");

        // ---- Control clue strength by exponent ----
        // We filter candidates by "tier" depending on exponent.
        // Higher exponent => only oblique clues.
        // Lower exponent => allow stronger clues.
        List<string> filtered = new List<string>();

        if (currentExponentAfterDrop >= 8)
        {
            // Very oblique only
            filtered.Add(candidates[0]);
        }
        else if (currentExponentAfterDrop >= 5)
        {
            // Oblique + light math constraints
            filtered.Add(candidates[0]);
            filtered.Add(candidates[1]);
            filtered.Add(candidates[2]);
            filtered.Add(candidates[3]);
        }
        else if (currentExponentAfterDrop >= 3)
        {
            // Add modular hints
            filtered.AddRange(new[] { candidates[0], candidates[1], candidates[2], candidates[3], candidates[4], candidates[5], candidates[6] });
        }
        else
        {
            // Best quality: allow range-ish and the stronger flavor clue
            filtered.AddRange(candidates);
        }

        // Remove already used
        filtered.RemoveAll(c => used.Contains(c));
        if (filtered.Count == 0) return "";

        string chosen = filtered[rng.Next(filtered.Count)];
        used.Add(chosen);
        return chosen;
    }

    static void WinReveal(string name)
    {
        Console.WriteLine($"\n{name}… the Oracle stops resisting.");
        Console.WriteLine("The ritual collapses into a single, quiet truth.\n");

        Console.WriteLine("🏆 YOU WIN: THE MASTER NUMBER IS 42 🏆\n");

        Console.WriteLine("In Douglas Adams’ *The Hitchhiker’s Guide to the Galaxy*,");
        Console.WriteLine("a supercomputer (Deep Thought) is asked for the Answer to the");
        Console.WriteLine("Ultimate Question of Life, the Universe, and Everything.");
        Console.WriteLine("After an absurdly long calculation, it delivers:\n");
        Console.WriteLine("42.\n");
        Console.WriteLine("The punchline is that an Answer without the right Question");
        Console.WriteLine("is useless—so the quest for meaning continues.\n");

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}