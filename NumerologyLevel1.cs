using System;
using System.Numerics;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== THE BRUTAL ORACLE ===");
        Console.WriteLine("You have 5 guesses to divine the Master Number.\n");

        Console.Write("Enter your seeker name: ");
        string name = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(name)) name = "Seeker";

        const int MasterNumber = 42;
        const int MaxTurns = 5;

        // The hidden "ritual" layer: base^exponent (base stays secret)
        Random rng = new Random();
        int baseNumber = rng.Next(1, 101); // 1..100
        int exponent = 1;

        Console.WriteLine($"\nWelcome, {name}.");
        Console.WriteLine("The Oracle listens. The ritual escalates.\n");

        for (int turn = 1; turn <= MaxTurns; turn++)
        {
            Console.WriteLine($"Turn {turn}/{MaxTurns}  |  Ritual Exponent: {exponent}");
            Console.Write("Speak a number: ");

            string input = Console.ReadLine()?.Trim() ?? "";
            if (!BigInteger.TryParse(input, out BigInteger guess))
            {
                Console.WriteLine("The Oracle does not understand that shape. (Enter a whole number.)\n");
                turn--; // don't waste a turn on invalid input
                continue;
            }

            // WIN CONDITION: guess the master number
            if (guess == MasterNumber)
            {
                WinReveal(name);
                return;
            }

            // Subtle hint system based on what they guessed (doesn't reveal the master directly)
            GiveSubtleHint(guess);

            // Background brutal ritual logic (does NOT affect win condition directly)
            // It exists to create tension / progression / "the machine reacts"
            BigInteger currentTarget = BigInteger.Pow(baseNumber, exponent);
            BigInteger tolerance = GetGlidingTolerance(currentTarget, exponent);
            BigInteger diff = BigInteger.Abs(guess - currentTarget);

            bool canDrop = exponent > 1;
            if (canDrop && diff <= tolerance)
            {
                exponent--;
                Console.WriteLine("The air stills for a moment. The ritual slackens.\n");
            }
            else
            {
                exponent++;
                Console.WriteLine("The pressure increases. The ritual intensifies.\n");
            }

            // Keep exponent from getting silly (game is only 5 turns anyway)
            if (exponent < 1) exponent = 1;
            if (exponent > 12) exponent = 12;
        }

        Console.WriteLine("The Oracle closes its eye.");
        Console.WriteLine("You leave with fragments, not truth.\n");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static void GiveSubtleHint(BigInteger guess)
    {
        // We keep these intentionally poetic and indirect.
        // They’re triggered by properties of the player's guess, not by closeness to 42.
        // That way, the hints feel like “omens” rather than a standard number puzzle.

        string s = guess.ToString();
        bool twoDigit = (guess >= 10 && guess <= 99) || (guess <= -10 && guess >= -99);
        bool even = (guess % 2 == 0);
        bool has4 = s.Contains('4');
        bool has2 = s.Contains('2');

        int omenCount = 0;

        if (twoDigit)
        {
            Console.WriteLine("Omen: The answer wears two faces.");
            omenCount++;
        }
        if (even)
        {
            Console.WriteLine("Omen: Balance. Symmetry. A paired rhythm.");
            omenCount++;
        }
        if (has4)
        {
            Console.WriteLine("Omen: A right-angled shape flashes behind your eyes.");
            omenCount++;
        }
        if (has2)
        {
            Console.WriteLine("Omen: You hear a duet—two notes insisting they belong together.");
            omenCount++;
        }

        if (omenCount == 0)
        {
            Console.WriteLine("Omen: Only static. No pattern holds.");
        }
    }

    // Tolerance that glides up as targets get bigger AND as exponent climbs.
    // This makes the “downshift” more plausible later, without becoming trivial early.
    static BigInteger GetGlidingTolerance(BigInteger target, int exponent)
    {
        // Percent window grows with exponent:
        // e1: 4%, e2: 6%, e3: 8%, ... capped at 22%
        int percent = Math.Min(22, 4 + (exponent - 1) * 2);

        BigInteger tol = (target * percent) / 100;

        // Minimum tolerance so early game isn't impossible.
        if (tol < 10) tol = 10;

        return tol;
    }

    static void WinReveal(string name)
    {
        Console.WriteLine($"\n{name}… the Oracle stops resisting.");
        Console.WriteLine("The ritual collapses into a single, quiet truth.\n");

        Console.WriteLine("🏆 YOU WIN: THE MASTER NUMBER IS 42 🏆\n");

        Console.WriteLine("In Douglas Adams’ *The Hitchhiker’s Guide to the Galaxy*,");
        Console.WriteLine("a supercomputer named Deep Thought is asked for the Answer");
        Console.WriteLine("to the Ultimate Question of Life, the Universe, and Everything.");
        Console.WriteLine("After an absurdly long calculation, it delivers the answer:\n");
        Console.WriteLine("42.\n");
        Console.WriteLine("The joke (and the charm) is that the Answer is meaningless");
        Console.WriteLine("without knowing the right Question—so the search continues.\n");

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}