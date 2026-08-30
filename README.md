# Educational Recommendation System (ERS)

## Overview
This project is a hybrid educational course recommendation system designed to help users find relevant educational content based on their current skills, development goals, and rating history. The system addresses the "information overload" problem in digital education by dynamically adapting to the user's needs. It is built using a strict N-Tier architecture and operates in real-time, providing an automated selection of courses.

## Key Features
* **Hybrid Recommendation Engine**: Combines content-based filtering for new users and collaborative filtering for users with an established rating history.
* **Cold Start Mitigation**: Seamlessly relies on content analysis of a user's stated skills when there is no prior rating data available.
* **ETL Data Pipeline**: Features an automated pipeline utilizing Strategy and Adapter design patterns to parse, transform, and load course datasets.
* **RESTful API Integration**: Provides robust endpoints for user management, skill tracking, rating submissions, and recommendation retrieval.

## Architecture and Algorithms
* **Backend Framework**: Developed in C# on the .NET 8.0 platform, ensuring high performance and efficient memory management.
* **Database Management**: Utilizes MySQL 8.0 with ADO.NET for direct, optimized, and asynchronous database interactions.
* **Content-Based Filtering**: Employs Term Frequency-Inverse Document Frequency (TF-IDF) to convert course descriptions into vector spaces to match them with user interests.
* **Relevance Matching**: Uses Cosine Similarity to calculate the alignment between a user's skill vector and a course vector in a multi-dimensional space.
* **Collaborative Filtering**: Applies the K-Nearest Neighbors (KNN) algorithm to identify similar learners and predict course ratings based on community experience.

## Project Structure
* **`ERS.API`**: The web access and routing layer that acts as the entry point for the system. It handles HTTP requests, input validation, and returns responses in JSON format.
* **`ERS.Core`**: The independent business logic core containing the recommendation algorithms, ETL pipeline managers, and the MySQL data access repository.
* **`ERS.Core.Tests`**: A dedicated unit testing project designed to automatically verify the accuracy of the mathematical algorithms (TF-IDF, KNN, and Similarities).
