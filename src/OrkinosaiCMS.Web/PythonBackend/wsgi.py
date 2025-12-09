"""
WSGI entry point for Zoota AI Backend
This file is used by gunicorn in production environments
"""
from app import app

# Expose the Flask application for gunicorn
application = app

if __name__ == "__main__":
    # For development/testing only
    app.run()