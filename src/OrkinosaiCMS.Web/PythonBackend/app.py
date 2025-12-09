"""
Zoota AI Assistant Backend - Azure OpenAI Integration with SQL Server Training Data
Reads configuration from appsettings.json
"""
import os
import json
import logging
import re
from pathlib import Path
from flask import Flask, request, jsonify
from flask_cors import CORS
from openai import AzureOpenAI
import pyodbc
from urllib.parse import quote_plus

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

app = Flask(__name__)
CORS(app)

# Global configuration
config = None
azure_client = None
db_connection = None

def load_configuration():
    """Load configuration from appsettings.json"""
    global config
    
    # Try to find appsettings.json
    config_paths = [
        Path(__file__).parent.parent / 'appsettings.json',  # ../appsettings.json
        Path('appsettings.json'),  # Current directory
        Path('/home/site/wwwroot/appsettings.json'),  # Azure App Service path
    ]
    
    config_file = None
    for path in config_paths:
        if path.exists():
            config_file = path
            logger.info(f"Found configuration file at: {path}")
            break
    
    if not config_file:
        logger.error("Could not find appsettings.json")
        return None
    
    try:
        with open(config_file, 'r') as f:
            config = json.load(f)
            logger.info("Configuration loaded successfully")
            return config
    except Exception as e:
        logger.error(f"Error loading configuration: {e}")
        return None

def initialize_azure_client():
    """Initialize Azure OpenAI client from configuration"""
    global azure_client, config
    
    if not config:
        logger.error("Configuration not loaded, cannot initialize Azure client")
        return False
    
    try:
        azure_config = config.get('AzureOpenAI', {})
        
        endpoint = azure_config.get('Endpoint')
        api_key = azure_config.get('ApiKey')
        api_version = azure_config.get('ApiVersion', '2024-08-01-preview')
        
        if not endpoint or not api_key:
            logger.warning("Azure OpenAI credentials not configured in appsettings.json")
            return False
        
        # Check if using placeholder values
        if 'your-resource-name' in endpoint or 'your-api-key' in api_key:
            logger.warning("Azure OpenAI using placeholder values - responses will be mocked")
            return False
        
        azure_client = AzureOpenAI(
            azure_endpoint=endpoint,
            api_key=api_key,
            api_version=api_version
        )
        
        logger.info("Azure OpenAI client initialized successfully")
        return True
        
    except Exception as e:
        logger.error(f"Error initializing Azure client: {e}")
        return False

def initialize_database():
    """Initialize database connection from configuration"""
    global db_connection, config
    
    if not config:
        logger.error("Configuration not loaded, cannot initialize database")
        return False
    
    if not config.get('DatabaseEnabled', False):
        logger.info("Database is disabled in configuration")
        return False
    
    try:
        connection_string = config.get('ConnectionStrings', {}).get('DefaultConnection', '')
        
        if not connection_string or 'YOUR-PASSWORD-HERE' in connection_string:
            logger.warning("Database connection string not properly configured")
            return False
        
        # Parse connection string for pyodbc
        # Example: Server=tcp:server.database.windows.net,1433;Initial Catalog=db;User ID=user;Password=pass;...
        server_match = re.search(r'Server=tcp:([^,;]+)', connection_string)
        database_match = re.search(r'Initial Catalog=([^;]+)', connection_string)
        user_match = re.search(r'User ID=([^;]+)', connection_string)
        password_match = re.search(r'Password=([^;]+)', connection_string)
        
        if not all([server_match, database_match, user_match, password_match]):
            logger.warning("Could not parse database connection string")
            return False
        
        server = server_match.group(1)
        database = database_match.group(1)
        username = user_match.group(1)
        password = password_match.group(1)
        
        # Build ODBC connection string
        driver = '{ODBC Driver 18 for SQL Server}'  # Try newer driver first
        odbc_conn_str = f'Driver={driver};Server=tcp:{server},1433;Database={database};Uid={username};Pwd={password};Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;'
        
        db_connection = pyodbc.connect(odbc_conn_str)
        logger.info("Database connection initialized successfully")
        return True
        
    except pyodbc.Error as e:
        # Try fallback to older driver
        try:
            driver = '{ODBC Driver 17 for SQL Server}'
            odbc_conn_str = f'Driver={driver};Server=tcp:{server},1433;Database={database};Uid={username};Pwd={password};Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;'
            db_connection = pyodbc.connect(odbc_conn_str)
            logger.info("Database connection initialized with ODBC Driver 17")
            return True
        except Exception as e2:
            logger.warning(f"Database connection failed (will use config-based knowledge): {e2}")
            return False
    except Exception as e:
        logger.warning(f"Error initializing database (will use config-based knowledge): {e}")
        return False

def get_training_data_from_db():
    """Retrieve training data from SQL Server database"""
    global db_connection
    
    if not db_connection:
        return None
    
    try:
        cursor = db_connection.cursor()
        training_context = []
        
        # Get website content
        cursor.execute("""
            SELECT TOP 50 Title, Content, Url, Description 
            FROM WebsiteContents 
            WHERE IsActive = 1 
            ORDER BY LastCrawled DESC
        """)
        
        for row in cursor.fetchall():
            title, content, url, description = row
            # Limit content size to avoid token limits
            content_preview = content[:1000] if content else ""
            training_context.append(f"Page: {title}\nURL: {url}\n{description or ''}\n{content_preview}")
        
        # Get LinkedIn profile
        cursor.execute("""
            SELECT TOP 1 FullName, Headline, Summary, Experience, Skills, Location
            FROM LinkedInProfiles 
            WHERE IsActive = 1 
            ORDER BY LastUpdated DESC
        """)
        
        linkedin_row = cursor.fetchone()
        if linkedin_row:
            full_name, headline, summary, experience, skills, location = linkedin_row
            linkedin_text = f"""
LinkedIn Profile: {full_name}
Headline: {headline}
Location: {location}
Summary: {summary or 'N/A'}
Key Skills: {skills or 'N/A'}
Experience: {experience[:500] if experience else 'N/A'}
"""
            training_context.append(linkedin_text)
        
        # Get additional training data
        cursor.execute("""
            SELECT TOP 20 Category, Content, Source
            FROM TrainingData
            WHERE IsActive = 1
            ORDER BY Priority DESC, CreatedAt DESC
        """)
        
        for row in cursor.fetchall():
            category, content, source = row
            training_context.append(f"[{category}] {content[:500]}")
        
        cursor.close()
        
        if training_context:
            logger.info(f"Retrieved {len(training_context)} training data items from database")
            return "\n\n---\n\n".join(training_context)
        
        return None
        
    except Exception as e:
        logger.error(f"Error retrieving training data from database: {e}")
        return None

def detect_language(text):
    """Simple language detection for Turkish vs English
    
    Returns 'tr' only when there are clear Turkish indicators.
    Defaults to 'en' (English) for ambiguous cases.
    """
    import re
    
    # Turkish-specific characters (lowercase only, since we check against lowercased text)
    # These are the most reliable indicators
    turkish_chars = set('çğıöşü')
    
    # Strong Turkish indicators - distinctive words that clearly indicate Turkish
    # Only include words that are uniquely Turkish and unlikely to appear in English
    strong_turkish_words = {'merhaba', 'nasıl', 'nedir', 'nelerdir', 'hakkında',
                            'hizmetleriniz', 'çözümleriniz', 'danışmanlık', 'misiniz', 
                            'musunuz', 'miyim'}
    
    # Turkish suffixes (common question/verb endings)
    # Only check these for longer words to avoid false positives
    turkish_suffixes = ['misiniz', 'musunuz', 'midir', 'müdür', 'nelerdir']
    
    text_lower = text.lower().strip()
    
    # If text is too short, default to English
    if len(text_lower) < 3:
        return 'en'
    
    # Priority 1: Check for Turkish-specific characters (most reliable)
    if any(char in text_lower for char in turkish_chars):
        return 'tr'
    
    # Extract words (remove punctuation)
    words = set(re.findall(r'\b\w+\b', text_lower))
    
    # Priority 2: Check for strong Turkish words
    if words.intersection(strong_turkish_words):
        return 'tr'
    
    # Priority 3: Check for Turkish suffixes at word boundaries to avoid false positives
    # Use word boundaries to ensure we match complete words ending with these suffixes
    for word in words:
        if any(word.endswith(suffix) for suffix in turkish_suffixes):
            return 'tr'
    
    # Default to English for ambiguous or unclear cases
    return 'en'

def build_enhanced_system_prompt(user_message=None):
    """Build system prompt with training data from database and language-specific instructions"""
    global config
    
    zoota_config = config.get('Zoota', {}) if config else {}
    base_prompt = zoota_config.get('SystemPrompt', '')
    
    # Detect language if user message is provided
    if user_message:
        detected_lang = detect_language(user_message)
        if detected_lang == 'tr':
            base_prompt += "\n\nThe user is asking in TURKISH. You MUST respond COMPLETELY in Turkish. Do not mix languages."
        else:
            base_prompt += "\n\nThe user is asking in ENGLISH. You MUST respond in ENGLISH. This is the default language."
    else:
        # When no user message, explicitly default to English
        base_prompt += "\n\nDEFAULT LANGUAGE: ENGLISH. Respond in English unless the user clearly uses Turkish."
    
    # Get training data from database
    training_data = get_training_data_from_db()
    
    if training_data:
        enhanced_prompt = f"""{base_prompt}

You have been trained on the following information about OrkinosAI and its team:

{training_data}

Use this information to provide accurate, specific, and helpful responses. Always cite the website or LinkedIn profile when relevant."""
        return enhanced_prompt
    else:
        # Fallback to config-based knowledge
        return base_prompt

def get_mock_response(user_message):
    """Generate mock response when Azure OpenAI is not configured"""
    message_lower = user_message.lower()
    
    # Detect language
    detected_lang = detect_language(user_message)
    is_turkish = detected_lang == 'tr'
    
    # Get knowledge base from config
    zoota_config = config.get('Zoota', {}) if config else {}
    kb = zoota_config.get('KnowledgeBase', {})
    
    # Turkish responses
    if is_turkish:
        if any(word in message_lower for word in ['merhaba', 'selam', 'hey']):
            return "Merhaba! Ben Zoota, yapay zeka asistanınızım. Şu anda demo modunda çalışıyorum. OrkinosAI hakkında size nasıl yardımcı olabilirim?"
        
        elif 'orkinosai' in message_lower or 'hakkında' in message_lower:
            services = kb.get('Services', [])
            services_text = '\n'.join([f"• {s}" for s in services[:5]])
            return f"OrkinosAI, şu alanlarda uzmanlaşmış bir teknoloji danışmanlık şirketidir:\n\n{services_text}\n\nAzure, AI/ML ve modern web geliştirme konularında uzmanlığa sahip bir Microsoft İş Ortağıyız."
        
        elif 'hizmet' in message_lower or 'servis' in message_lower:
            services = kb.get('Services', [])
            services_text = '\n'.join([f"• {s}" for s in services])
            return f"OrkinosAI kapsamlı teknoloji hizmetleri sunmaktadır:\n\n{services_text}\n\nHangi hizmet hakkında daha fazla bilgi edinmek istersiniz?"
        
        elif 'yapay zeka' in message_lower or 'ai' in message_lower or 'makine öğrenmesi' in message_lower:
            return "Azure OpenAI, TensorFlow ve PyTorch kullanarak Yapay Zeka ve Makine Öğrenmesi çözümleri sunuyoruz. Uzmanlık alanlarımız arasında konuşma yapay zekası, özel yapay zeka modelleri ve akıllı otomasyon sistemleri bulunmaktadır. İşletmelerin yapay zeka gücünden yararlanmalarına yardımcı oluyoruz."
        
        elif 'azure' in message_lower or 'bulut' in message_lower:
            return "Azure Bulut Çözümlerinde uzmanız! Bulut geçişi, mimari tasarım, DevOps ve sürekli Azure yönetimi hizmetleri sunuyoruz. Microsoft İş Ortağı olarak, kuruluşların Azure'un tüm gücünden yararlanmalarına yardımcı oluyoruz."
        
        elif 'iletişim' in message_lower or 'ulaş' in message_lower or 'email' in message_lower or 'telefon' in message_lower:
            website = kb.get('Website', 'https://www.orkinosai.com')
            return f"Bize web sitemiz {website} üzerinden veya sitemizdeki iletişim formu aracılığıyla ulaşabilirsiniz. Teknoloji ihtiyaçlarınız hakkında konuşmak isteriz!"
        
        else:
            return "OrkinosAI'ın hizmetleri, uzmanlık alanları veya teknoloji çözümleri hakkında size yardımcı olmaktan mutluluk duyarım. Web geliştirme, AI/ML, Azure bulut, SharePoint/M365 veya diğer hizmetlerimiz hakkında soru sorabilirsiniz!"
    
    # English responses
    else:
        if any(word in message_lower for word in ['hello', 'hi', 'hey']):
            return "Hello! I'm Zoota, your AI assistant. I'm currently running in demo mode. How can I help you learn about OrkinosAI?"
        
        elif 'orkinosai' in message_lower or 'about' in message_lower:
            services = kb.get('Services', [])
            services_text = '\n'.join([f"• {s}" for s in services[:5]])
            return f"OrkinosAI is a technology consultancy specializing in:\n\n{services_text}\n\nWe're a Microsoft Partner with expertise in Azure, AI/ML, and modern web development."
        
        elif 'service' in message_lower:
            services = kb.get('Services', [])
            services_text = '\n'.join([f"• {s}" for s in services])
            return f"OrkinosAI offers comprehensive technology services:\n\n{services_text}\n\nWhich service would you like to know more about?"
        
        elif 'web' in message_lower and ('develop' in message_lower or 'site' in message_lower):
            return "Yes! We specialize in web development using modern technologies like .NET, Blazor, React, and more. We build custom web applications, CMS solutions, and enterprise web platforms. We also offer web CMS solutions using Umbraco and custom frameworks."
        
        elif 'ai' in message_lower or 'artificial intelligence' in message_lower or 'machine learning' in message_lower:
            return "We provide AI & Machine Learning solutions using Azure OpenAI, TensorFlow, and PyTorch. Our expertise includes conversational AI (like me!), custom AI models, intelligent automation, and AI-powered business solutions to help organizations leverage AI effectively."
        
        elif 'azure' in message_lower or 'cloud' in message_lower:
            return "We're Azure experts! We offer cloud migration, architecture design, DevOps, security, and ongoing Azure management. As a Microsoft Partner, we help organizations move to Azure and optimize their cloud infrastructure for performance and cost."
        
        elif 'sharepoint' in message_lower or 'microsoft 365' in message_lower or 'm365' in message_lower:
            return "We provide comprehensive SharePoint and Microsoft 365 solutions including custom development, workflow automation, migration services, and integration with other systems. We help organizations maximize their Microsoft 365 investment."
        
        elif 'contact' in message_lower or 'reach' in message_lower or 'email' in message_lower or 'phone' in message_lower:
            website = kb.get('Website', 'https://www.orkinosai.com')
            return f"You can reach OrkinosAI through our website at {website} or use the contact form on our website. We'd love to discuss how we can help with your technology needs!"
        
        else:
            return "I'd be happy to help you learn more about OrkinosAI's services, expertise, or technology solutions. Feel free to ask about our web development, AI/ML, Azure cloud, SharePoint/M365, or other services!"

@app.route('/health', methods=['GET'])
def health():
    """Health check endpoint"""
    try:
        return jsonify({
            'status': 'healthy',
            'service': 'Zoota AI Backend',
            'version': '2.0.0',
            'azure_configured': azure_client is not None,
            'config_loaded': config is not None,
            'database_enabled': db_connection is not None
        })
    except Exception as e:
        logger.error(f"Health check error: {e}")
        return jsonify({
            'status': 'degraded',
            'service': 'Zoota AI Backend',
            'error': str(e)
        }), 500

@app.route('/api/chat', methods=['POST'])
def chat():
    """Chat endpoint for Zoota AI Assistant"""
    try:
        data = request.get_json()
        user_message = data.get('message', '').strip()
        conversation_history = data.get('history', [])
        
        if not user_message:
            return jsonify({'error': 'Message is required'}), 400
        
        logger.info(f"Received chat message: {user_message[:50]}...")
        
        # If Azure client is configured, use it
        if azure_client:
            try:
                azure_config = config.get('AzureOpenAI', {})
                
                # Build enhanced system prompt with training data and language detection
                enhanced_prompt = build_enhanced_system_prompt(user_message)
                
                # Build messages for OpenAI
                messages = [
                    {"role": "system", "content": enhanced_prompt}
                ]
                
                # Add conversation history (FIXED: only add if there's actual history)
                if conversation_history:
                    for msg in conversation_history[-10:]:  # Last 10 messages
                        messages.append({
                            "role": msg.get('role', 'user'),
                            "content": msg.get('content', '')
                        })
                
                # Add current message
                messages.append({"role": "user", "content": user_message})
                
                # Call Azure OpenAI
                response = azure_client.chat.completions.create(
                    model=azure_config.get('DeploymentName', 'gpt-4o'),
                    messages=messages,
                    max_tokens=azure_config.get('MaxTokens', 800),
                    temperature=azure_config.get('Temperature', 0.7),
                    top_p=azure_config.get('TopP', 0.95),
                    frequency_penalty=azure_config.get('FrequencyPenalty', 0),
                    presence_penalty=azure_config.get('PresencePenalty', 0)
                )
                
                assistant_message = response.choices[0].message.content
                logger.info("Azure OpenAI response generated successfully")
                
                return jsonify({
                    'message': assistant_message,
                    'source': 'azure_openai'
                })
                
            except Exception as e:
                logger.error(f"Azure OpenAI error: {e}")
                # Fall back to mock response
                mock_response = get_mock_response(user_message)
                return jsonify({
                    'message': mock_response,
                    'source': 'mock_fallback',
                    'error': str(e)
                })
        
        # Use mock response when Azure is not configured
        else:
            mock_response = get_mock_response(user_message)
            return jsonify({
                'message': mock_response,
                'source': 'mock'
            })
            
    except Exception as e:
        logger.error(f"Error processing chat request: {e}")
        return jsonify({'error': 'Internal server error'}), 500

@app.route('/api/config', methods=['GET'])
def get_config():
    """Get public configuration (non-sensitive data only)"""
    if not config:
        return jsonify({'error': 'Configuration not loaded'}), 500
    
    zoota_config = config.get('Zoota', {})
    
    return jsonify({
        'name': zoota_config.get('Name', 'Zoota AI Assistant'),
        'version': zoota_config.get('Version', '1.0.0'),
        'welcomeMessage': zoota_config.get('WelcomeMessage', ''),
        'azureConfigured': azure_client is not None
    })

# Initialize configuration on module load
logger.info("=" * 50)
logger.info("Zoota AI Backend - Starting Initialization")
logger.info("=" * 50)

config = load_configuration()
if config:
    logger.info("✓ Configuration loaded successfully")
    
    # Initialize Azure OpenAI and Database
    azure_initialized = initialize_azure_client()
    db_initialized = initialize_database()
    
    # Log initialization status
    logger.info(f"Azure OpenAI: {'✓ Configured' if azure_initialized else '⚠ Not configured (will use mock responses)'}")
    logger.info(f"Database: {'✓ Connected' if db_initialized else '⚠ Not connected (will use config-based knowledge)'}")
else:
    logger.error("✗ Configuration failed to load")

logger.info("=" * 50)
logger.info("Zoota AI Backend - Initialization Complete")
logger.info("=" * 50)

if __name__ == '__main__':
    # Get port from config or environment
    port = 8000
    if config:
        port = config.get('PythonBackend', {}).get('Port', 8000)
    
    port = int(os.environ.get('PORT', port))
    
    logger.info(f"Starting Zoota AI Backend on port {port}")
    logger.info("Press CTRL+C to stop")
    app.run(host='0.0.0.0', port=port, debug=False)
