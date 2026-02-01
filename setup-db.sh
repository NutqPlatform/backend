#!/bin/bash

# Build the solution
echo "Building solution..."
cd /home/sama/project/backend
dotnet build

if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi

# Update database
echo "Applying migrations..."
cd /home/sama/project/backend/Nutq.Web
dotnet ef database update --project ../Nutq.Infrastructure

if [ $? -ne 0 ]; then
    echo "Migration failed!"
    exit 1
fi

echo "Setup complete!"
