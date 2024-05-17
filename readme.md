# Introduction to Backend Development with .NET

## Setup Instructions

### Step 1: Create a New Project

To start, create a new project using the command line. Open your terminal and run the following command:

```bash
dotnet new webapi -n blog-website-api
```

### Step 2: Change into your project directory:

```bash
cd blog-website-api
```

### Step 3: Install Dependencies

Install the necessary libraries and dependencies by running the following commands:

```bash
dotnet add package BCrypt.Net-Next --version 4.0.3
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.5
dotnet add package Microsoft.AspNetCore.OpenApi --version 8.0.1
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.5
dotnet add package MongoDB.Driver
dotnet add package Pomelo.EntityFrameworkCore.MySql --version 8.0.2
dotnet add package Swashbuckle.AspNetCore --version 6.6.1
dotnet add package Swashbuckle.AspNetCore.Annotations --version 6.6.1
```

### Step 4: Create Folder Structure

Set up the basic folder structure for your project by creating the necessary directories:

```bash
mkdir controllers
mkdir data
mkdir DTOs
mkdir models
```

### Step 5: Create .gitignore file

### Step 6: Initialize git repository

```bash
git init
git add .
git commit -m "initial commit"
git branch -M main
git remote add origin https://github.com/<github-username>/<your-git-repo>.git
git push -u origin main
```
