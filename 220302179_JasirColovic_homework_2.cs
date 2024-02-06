using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

class Program
{
    static void Main()
    {
        string jsonFilePath = "questions.json";

        if (!File.Exists(jsonFilePath))
        {
            Console.WriteLine($"Error: The file '{jsonFilePath}' was not found.");
            return;
        }
        string json = File.ReadAllText(jsonFilePath);

        List<Question> allQuestions;
        try
        {
            allQuestions = JsonConvert.DeserializeObject<List<Question>>(json);
        }
        catch (JsonException)
        {
            Console.WriteLine("Error: Unable to deserialize JSON. Check the file format.");
            return;
        }

        // prompt the user for difficulty level
        Console.Write("Choose difficulty level (easy/hard): ");
        string difficultyLevel = Console.ReadLine().ToLower();

        List<Question> selectedQuestions;

        // get questions based on difficulty level
        if (difficultyLevel == "easy")
        {
            selectedQuestions = GetRandomQuestions(allQuestions.FindAll(q => q.Difficulty == "easy"), 5);
        }
        else if (difficultyLevel == "hard")
        {
            selectedQuestions = GetRandomQuestions(allQuestions.FindAll(q => q.Difficulty == "hard"), 5);
        }
        else
        {
            Console.WriteLine("Invalid difficulty level. Please choose 'easy' or 'hard'.");
            return;
        }

        int correctAnswers = 0;
        int lastAttemptedQuestionIndex = -1;

        Stopwatch quizStopwatch = new Stopwatch();
        quizStopwatch.Start();

        foreach (var question in selectedQuestions)
        {
            Console.WriteLine($"You have 3 seconds to answer this question: {question.QuestionText}");//and if you modify time also modify this

            Stopwatch questionStopwatch = new Stopwatch();
            questionStopwatch.Start();

            for (int i = 0; i < question.Options.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {question.Options[i]}");
            }

            Console.Write("Your Answer: ");
            string userAnswer = Console.ReadLine();

            questionStopwatch.Stop();

            if (questionStopwatch.Elapsed.TotalSeconds > 3)//here you can modify the time 
            {
                Console.WriteLine("Time's up! Moving on to the next question.\n");
                continue;
            }

            if (int.TryParse(userAnswer, out int userChoice) && userChoice >= 1 && userChoice <= question.Options.Count)
            {
                string selectedOption = question.Options[userChoice - 1];
                if (selectedOption == question.CorrectAnswer)
                {
                    correctAnswers++;
                    Console.WriteLine("Correct!\n");
                }
                else
                {
                    Console.WriteLine($"Incorrect. The correct answer is: {question.CorrectAnswer}\n");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid option.\n");
            }

            Console.WriteLine($"Correct Answers: {correctAnswers}\n");
            lastAttemptedQuestionIndex++;
        }

        quizStopwatch.Stop();

        Console.WriteLine($"{correctAnswers} out of 5 were completed successfully");
        Console.WriteLine($"Last Attempted Question Index: {lastAttemptedQuestionIndex}");
        Console.WriteLine($"Total Quiz Time: {quizStopwatch.Elapsed.TotalSeconds} seconds");

        // save progress to a JSON file
        SaveProgressToJson(correctAnswers, lastAttemptedQuestionIndex, quizStopwatch.Elapsed.TotalSeconds);
    }

    static void SaveProgressToJson(int correctAnswers, int lastAttemptedQuestionIndex, double totalQuizTime)
    {
        QuizProgress progress = new QuizProgress
        {
            CorrectAnswers = correctAnswers,
            LastAttemptedQuestionIndex = lastAttemptedQuestionIndex,
            TotalQuizTime = totalQuizTime
        };

        string json = JsonConvert.SerializeObject(progress, Formatting.Indented);
        File.WriteAllText("quiz_progress.json", json);
    }

    static List<Question> GetRandomQuestions(List<Question> questions, int count)
    {
        Random random = new Random();
        return questions.OrderBy(q => random.Next()).Take(count).ToList();
    }
}

public class Question
{
    public string QuestionText { get; set; }
    public List<string> Options { get; set; }
    public string CorrectAnswer { get; set; }
    public string Difficulty { get; set; }
}

public class QuizProgress
{
    public int CorrectAnswers { get; set; }
    public int LastAttemptedQuestionIndex { get; set; }
    public double TotalQuizTime { get; set; }
}