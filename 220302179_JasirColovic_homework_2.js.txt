const readline = require('readline');
const path = require('path');
const fs = require('fs').promises;

const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout
});

const questionsPath = path.resolve(__dirname, 'questions.json');
const progressFilePath = path.resolve(__dirname, 'progress.txt');
let score = 0;
let lastAttemptedQuestionIndex = 0;

const displayAnswerResult = (isCorrect) => {
  console.log(`\n${isCorrect ? 'Correct!' : 'Incorrect.'}`);
};

const saveProgress = async (score, totalTimeInSeconds, lastAttemptedQuestionIndex) => {
  const progressData = `Score: ${score}\nTotal Time: ${totalTimeInSeconds.toFixed(2)} seconds\nLast Attempted Question Index: ${lastAttemptedQuestionIndex}`;
  
  try {
    await fs.writeFile(progressFilePath, progressData);
    console.log('\nProgress saved successfully.');
  } catch (error) {
    console.error('Error saving progress:', error);
  }
};

const startQuiz = (questions) => {
  const countdownTimerDuration = 3000; // Set the countdown timer duration in milliseconds (3 seconds)
  const totalTimerStart = Date.now();
  const totalQuestionsToAsk = 5; // Set the total number of questions to ask

  rl.question('Choose a difficulty level: easy or hard: ', (difficulty) => {
    const filteredQuestions = questions.filter(question => question.difficulty === difficulty);
    filteredQuestions.sort(() => Math.random() - 0.5);
    const selectedQuestions = filteredQuestions.slice(0, totalQuestionsToAsk);

    const askQuestion = (index) => {
      if (index < totalQuestionsToAsk && !rl.closed) {
        const question = selectedQuestions[index];

        console.log(`\nYou have 3 seconds to answer the following question:`);
        console.log(`Question ${index + 1}: ${question.QuestionText}\n${Array.isArray(question.options) ? question.options.join('\n') : 'Options not available'}`);

        const timeoutId = setTimeout(() => {
          console.log('\nTime is up!');
          displayAnswerResult(false); // Display incorrect for timeout
          askQuestion(index + 1);
        }, countdownTimerDuration);

        rl.question('Your Answer: ', (answer) => {
          clearTimeout(timeoutId); // Cancel the timeout

          const correctAnswers = Array.isArray(question.correctAnswer) ? question.correctAnswer : [question.correctAnswer];
          const isCorrect = correctAnswers.includes(answer.trim());

          displayAnswerResult(isCorrect);

          if (isCorrect) {
            score++;
          }

          lastAttemptedQuestionIndex = index;
          askQuestion(index + 1); // Ask the next question
        });
      } else {
        const totalTimerEnd = Date.now();
        const totalTimeInSeconds = (totalTimerEnd - totalTimerStart) / 1000;

        console.log(`\nYou got ${score} out of ${totalQuestionsToAsk} questions right.`);
        console.log(`Total time taken: ${totalTimeInSeconds.toFixed(2)} seconds.`);
        rl.close();

        // Save progress
        saveProgress(score, totalTimeInSeconds, lastAttemptedQuestionIndex);
      }
    };

    // Start asking questions
    askQuestion(lastAttemptedQuestionIndex);
  });
};

// Read questions from file
fs.readFile(questionsPath, 'utf-8')
  .then(questions => JSON.parse(questions))
  .then(startQuiz)
  .catch(error => {
    console.error('Error:', error);
  });
