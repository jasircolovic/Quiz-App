import json
import random
import time
from inputimeout import inputimeout, TimeoutOccurred

# Config
DEFAULT_COUNTDOWN_TIME = 3

class TimeoutError(Exception):
    pass

def load_questions(file_path):
    with open(file_path) as file:
        questions = json.load(file)
    return questions

def randomize_questions(questions, difficulty):
    questions = [q for q in questions if q['difficulty'] == difficulty]
    random.shuffle(questions)
    return questions[:5]

def countdown_timer(countdown_time):
    for remaining_time in range(countdown_time, 0, -1):
        print(f"Time left: {remaining_time} seconds", end='\r')
        time.sleep(1)
    else:
        print("\nTime's up!")

def get_user_input(prompt, timeout=DEFAULT_COUNTDOWN_TIME):
    print(prompt, end='', flush=True)
    start_time = time.time()
    user_input = ''
    while time.time() - start_time < timeout:
        if user_input := input():
            return user_input
    return None

def ask_questions(questions, timeout=3):
    correct_answers = 0
    total_questions = len(questions)

    for i, question in enumerate(questions, start=1):
        print(f"Question {i}: {question['question']}")

        # check the type of question
        if 'options' in question:
            for j, option in enumerate(question['options'], start=1):
                print(f"{option}")
            correct_answer = question['correct_answer']
        elif 'correct_answer' in question and isinstance(question['correct_answer'], bool):
            print("1. True\n2. False")
            correct_answer = 1 if question['correct_answer'] else 2
        elif 'correct_answer' in question and isinstance(question['correct_answer'], str):
            correct_answer = question['correct_answer']
        else:
            print("Unknown question type.")
            continue

        try:
            user_input = inputimeout(prompt="Enter your answer: ", timeout=timeout)
            if 'options' in question:
                user_answer = int(user_input)
            else:
                user_answer = user_input

            if user_answer == correct_answer:
                correct_answers += 1

        except TimeoutOccurred:
            print(f"You did not answer in {timeout} seconds. Moving on to the next question.")

        print(f"Your current score is: {correct_answers} out of {i}\n")

    return correct_answers

def main():
    file_path = "questions.json"
    progress_file_path = "progress.txt"
    questions = load_questions(file_path)
    difficulty = input("Enter difficulty level (easy/hard): ")
    random_questions = randomize_questions(questions, difficulty)

    # Load progress from file
    try:
        with open(progress_file_path, 'r') as progress_file:
            progress = json.load(progress_file)
            start_index = progress['start_index']
            correct_answers = progress['correct_answers']
    except (FileNotFoundError, json.JSONDecodeError):
        start_index = 0
        correct_answers = 0

    # Ask questions and save progress
    for i in range(start_index, len(random_questions)):
        question = random_questions[i]
        if ask_questions([question]):  # Changed from ask_questions to ask_question
            correct_answers += 1

        # Save progress to file
        progress = {'start_index': i + 1, 'correct_answers': correct_answers}
        with open(progress_file_path, 'w') as progress_file:
            json.dump(progress, progress_file)

    print(f"\nYou answered {correct_answers} out of {len(random_questions)} questions correctly.")

if __name__ == "__main__":
    main()