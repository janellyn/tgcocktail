using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Exceptions;
using Newtonsoft.Json;

namespace telegrambot222
{
    public class CocktailRecipeBot
    {
        private string apiAddress = "https://cocktail-api1.herokuapp.com";

        TelegramBotClient botClient = new TelegramBotClient("5575398576:AAHGVlOv6wWQ1ext10ASf0nsX-CIo-qQ0K4");
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
        public string Name = "";
        public string UpdatedName = "";
        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMeAsync();
            Console.WriteLine($"Бот {botMe.Username} почав працювати");
            Console.ReadKey();
        }

        private Task HandlerError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Помилка в телеграм бот Апі:\n {apiRequestException.ErrorCode}" +
                $"\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                await HandlerMessageAsync(botClient, update.Message);
            }

            if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallBackQuery(botClient, update.CallbackQuery);
                return;
            }
        }

        public async Task<Cocktail> SearchCocktailByName(string name)
        {
            var _client = new HttpClient();
            _client.BaseAddress = new Uri(apiAddress);
            var result = await _client.GetAsync($"Cocktails/SearchCocktailByName?drink={name}");
            result.EnsureSuccessStatusCode();
            var content = result.Content.ReadAsStringAsync().Result;
            var cocktail = JsonConvert.DeserializeObject<Cocktail>(content);
            return cocktail;
        }

        public async Task<Cocktail> LookupARandomCocktail()
        {
            var _client = new HttpClient();
            _client.BaseAddress = new Uri(apiAddress);
            var result = await _client.GetAsync($"Cocktails/LookupARandomCocktail");
            result.EnsureSuccessStatusCode();
            var content = result.Content.ReadAsStringAsync().Result;
            var cocktail = JsonConvert.DeserializeObject<Cocktail>(content);
            return cocktail;
        }

        public async Task<Cocktail2> SearchByIngredient(string name)
        {
            var _client = new HttpClient();
            _client.BaseAddress = new Uri(apiAddress);
            var result = await _client.GetAsync($"Cocktails/SearchByIngredient?ingredient={name}");
            result.EnsureSuccessStatusCode();
            var content = result.Content.ReadAsStringAsync().Result;
            var cocktail = JsonConvert.DeserializeObject<Cocktail2>(content);
            return cocktail;
        }

        public async Task<Ingredients> GetIngredient(string name)
        {
            var _client = new HttpClient();
            _client.BaseAddress = new Uri(apiAddress);
            var result = await _client.GetAsync($"Cocktails/SearchIngredientByName?ingredient={name}");
            result.EnsureSuccessStatusCode();
            var content = result.Content.ReadAsStringAsync().Result;
            var cocktail = JsonConvert.DeserializeObject<Ingredients>(content);
            return cocktail;
        }

        public async Task<Database> GetFromDatabase(string name)
        {
            var _client = new HttpClient();
            _client.BaseAddress = new Uri(apiAddress);
            var result = await _client.GetAsync($"/Cocktails/GetFromDatabase?name={name}");
            var content = result.Content.ReadAsStringAsync().Result;
            var cocktail = JsonConvert.DeserializeObject<Database>(content);
            return cocktail;
        }

        public async Task<List<Database>> GetAll()
        {
            var _client = new HttpClient();
            _client.BaseAddress = new Uri(apiAddress);
            var result = await _client.GetAsync($"/Cocktails/GetAllFavorites");
            var content = result.Content.ReadAsStringAsync().Result;
            var cocktail = JsonConvert.DeserializeObject<List<Database>>(content);
            return cocktail;
        }
        public async Task<bool> DeleteFromDatabase(string name)
        {
            var _client = new HttpClient();
            _client.BaseAddress = new Uri(apiAddress);
            await _client.DeleteAsync($"/Cocktails/DeleteCocktail?name={name}");
            return true;
        }

        private async Task HandlerMessageAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "/start")
            {
                InlineKeyboardMarkup keyboardMarkup = new
                    (
                       new[]
                       {
                           new[]
                           {
                               InlineKeyboardButton.WithCallbackData("Get Cocktail", $"cocktail")
                           }, new[]
                           {
                               InlineKeyboardButton.WithCallbackData("Get Ingredient", $"ingredientinfo")
                           }
                       });

                await botClient.SendTextMessageAsync(message.Chat.Id, "I want to...", replyMarkup: keyboardMarkup);
                return;
            }
            else if (message.ReplyToMessage != null && message.ReplyToMessage.Text.Contains("Enter name of cocktail:"))
            {
                string name = message.Text;
                var cocktail = await SearchCocktailByName(name);
                Cocktail drinks = cocktail;

                if (drinks.drinks == null) await botClient.SendTextMessageAsync(message.Chat.Id, "Not found your cocktail :(");

                if (drinks.drinks != null)
                    foreach (var drink in drinks.drinks)
                        if (drink.strDrink != null)
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Name: {drink.strDrink}" +
                            $"\nAlternate: {drink.strDrinkAlternate}\nCategory: {drink.strCategory}\nAlcoholic: {drink.strAlcoholic}" +
                            $"\nGlass: {drink.strGlass}\nInstruction: {drink.strInstructions}" +
                            $"\nIngredients: {drink.strIngredient1}   { drink.strIngredient2}  { drink.strIngredient3}  { drink.strIngredient4}" +
                            $"  { drink.strIngredient5}  { drink.strIngredient6}  { drink.strIngredient7}  { drink.strIngredient8}" +
                            $"  { drink.strIngredient9}  { drink.strIngredient10}  { drink.strIngredient11}  { drink.strIngredient12}" +
                            $"  { drink.strIngredient13}  { drink.strIngredient14}  { drink.strIngredient15}" +
                            $"\nMeasure: {drink.strMeasure1}  { drink.strMeasure2}  { drink.strMeasure3}  { drink.strMeasure4}" +
                            $"  { drink.strMeasure5}  { drink.strMeasure6}  { drink.strMeasure7}  { drink.strMeasure8}" +
                            $"  {drink.strMeasure9}  { drink.strMeasure10}  { drink.strMeasure11}  { drink.strMeasure12}" +
                            $"  { drink.strMeasure13}  { drink.strMeasure14}  { drink.strMeasure15}\nImage: {drink.strDrinkThumb}");

                await botClient.SendTextMessageAsync(message.Chat.Id, "Open /start");
                return;
            }
            else if (message.ReplyToMessage != null && message.ReplyToMessage.Text.Contains("Enter name of ingredient:"))
            {
                string name = message.Text;
                var cocktail = await SearchByIngredient(name);
                Cocktail2 drinks = cocktail;

                if (drinks == null) await botClient.SendTextMessageAsync(message.Chat.Id, "Not found your ingredient :(");

                if (drinks != null)
                    foreach (var drink in drinks.drinks)
                        if (drink.strDrink != null)
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Name: {drink.strDrink}" +
                        $"\nImage: {drink.strDrinkThumb}");

                await botClient.SendTextMessageAsync(message.Chat.Id, "Open /start");
                return;
            }
            else if (message.ReplyToMessage != null && message.ReplyToMessage.Text.Contains("Enter ingredient:"))
            {
                string name = message.Text;
                var cocktail = await GetIngredient(name);
                Ingredients drinks = cocktail;

                if (drinks.ingredients == null) await botClient.SendTextMessageAsync(message.Chat.Id, "Not found your ingredient :(");

                if (drinks.ingredients != null)
                    foreach (var drink in drinks.ingredients)
                        if (drink.strIngredient != null)
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Name: {drink.strIngredient}" +
                        $"\nDescription: {drink.strDescription}\nType: {drink.strType}\nAlcohol: {drink.strAlcohol}");

                await botClient.SendTextMessageAsync(message.Chat.Id, "Open /start");
                return;
            }
            else if (message.ReplyToMessage != null && message.ReplyToMessage.Text.Contains("Enter name of cocktail in db:"))
            {
                string name = message.Text;
                var cocktail = await GetFromDatabase(name);
                Database drinks = cocktail;

                if (drinks.strDrink == null) await botClient.SendTextMessageAsync(message.Chat.Id, "Not found your cocktail :(");

                if (drinks.strDrink != null)
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Name: {drinks.strDrink}\nImage: {drinks.strDrinkThumb}");

                await botClient.SendTextMessageAsync(message.Chat.Id, "Open /start");
                return;
            }
            else if (message.ReplyToMessage != null && message.ReplyToMessage.Text.Contains("Enter name of cocktail to add in db:"))
            {
                string name = message.Text;
                Database result = new Database();

                try
                {
                    result.strDrink = message.Chat.Id.ToString();
                    var json = JsonConvert.SerializeObject(result);
                    var data = new StringContent(json, Encoding.UTF8, "application/json");
                    var _client = new HttpClient();
                    _client.BaseAddress = new Uri(apiAddress);
                    var result2 = await _client.PostAsync($"/Cocktails/AddToFavorites?name={name}", data);
                    var content = result2.Content.ReadAsStringAsync().Result;

                }
                catch (HttpRequestException)
                {
                    return;
                }

                await botClient.SendTextMessageAsync(message.Chat.Id, "Succesfully added!");

                await botClient.SendTextMessageAsync(message.Chat.Id, "Open /start");
                return;
            }
            else if (message.ReplyToMessage != null && message.ReplyToMessage.Text.Contains("Enter cocktail to delete:"))
            {
                string name = message.Text;
                await DeleteFromDatabase(name);

                await botClient.SendTextMessageAsync(message.Chat.Id, "Succesfully deleted!");

                await botClient.SendTextMessageAsync(message.Chat.Id, "Open /start");
                return;
            }
            else if (message.ReplyToMessage != null && message.ReplyToMessage.Text.Contains("Enter cocktail to update:"))
            {
                Name = message.Text;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter updated name:", replyMarkup: new ForceReplyMarkup { Selective = true });
            }
            else if (message.ReplyToMessage != null && message.ReplyToMessage.Text.Contains("Enter updated name:"))
            {
                UpdatedName = message.Text;
                var result = new Database
                {
                    strDrink = UpdatedName,
                    strDrinkThumb = ""
                };
                try
                {
                    var json = JsonConvert.SerializeObject(result);
                    var data = new StringContent(json, Encoding.UTF8, "application/jsonn");
                    var _client = new HttpClient();
                    _client.BaseAddress = new Uri(apiAddress);
                    var result2 = await _client.PutAsync($"/Cocktails/UpdateCocktail?name={Name}&updatedname={UpdatedName}", data);
                    var content = result2.Content.ReadAsStringAsync().Result;

                }
                catch (HttpRequestException)
                {
                    return;
                }

                await botClient.SendTextMessageAsync(message.Chat.Id, "Succesfully updated!");
                await botClient.SendTextMessageAsync(message.Chat.Id, "Open /start");
                return;
            }

        }
        async Task HandleCallBackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data.StartsWith("cocktail"))
            {
                InlineKeyboardMarkup keyboardMarkup = new
                (
                 new[]
                 {
                           new[]
                           {
                               InlineKeyboardButton.WithCallbackData("Search Cocktail by Name", $"searchcocktailbyname")
                           }, new[]
                           {
                               InlineKeyboardButton.WithCallbackData("Lookup A Random Cocktail", $"lookuparandom")
                           }, new[]
                           {
                               InlineKeyboardButton.WithCallbackData("Search By Ingredient", $"searchbyingredient")
                           }, new[]
                           {
                               InlineKeyboardButton.WithCallbackData("Get Cocktail from Database", $"getcocktailfromdb")
                           }, new[]
                           {
                               InlineKeyboardButton.WithCallbackData("Add To Database", $"addtodb")
                           }, new[]
                           {
                               InlineKeyboardButton.WithCallbackData("Get All Cocktails from Database", $"getallfromdb")
                           }, new[]
                           {
                               InlineKeyboardButton.WithCallbackData("Delete Cocktail from Database", $"deletefromdb")
                           }, new[]
                           {
                               InlineKeyboardButton.WithCallbackData("Update into Database", $"updateintodb")
                           }
                 }
                );

                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "You search Cocktail!", replyMarkup: keyboardMarkup);
                return;
            }

            if (callbackQuery.Data.StartsWith("ingredientinfo"))
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Enter ingredient:", replyMarkup: new ForceReplyMarkup { Selective = true });
                return;
            }

            if (callbackQuery.Data.StartsWith("searchcocktailbyname"))
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Enter name of cocktail:", replyMarkup: new ForceReplyMarkup { Selective = true });
                return;
            }

            if (callbackQuery.Data.StartsWith("lookuparandom"))
            {
                var cocktail = await LookupARandomCocktail();
                Cocktail drinks = cocktail;
                if (drinks.drinks == null) await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Not found your cocktail :(");

                if (drinks.drinks != null)
                    foreach (var drink in drinks.drinks)
                        if (drink.strDrink != null)
                            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Name: {drink.strDrink}" +
                            $"\nAlternate: {drink.strDrinkAlternate}\nCategory: {drink.strCategory}\nAlcoholic: {drink.strAlcoholic}" +
                            $"\nGlass: {drink.strGlass}\nInstruction: {drink.strInstructions}" +
                            $"\nIngredients: {drink.strIngredient1}   { drink.strIngredient2}  { drink.strIngredient3}  { drink.strIngredient4}" +
                            $"  { drink.strIngredient5}  { drink.strIngredient6}  { drink.strIngredient7}  { drink.strIngredient8}" +
                            $"  { drink.strIngredient9}  { drink.strIngredient10}  { drink.strIngredient11}  { drink.strIngredient12}" +
                            $"  { drink.strIngredient13}  { drink.strIngredient14}  { drink.strIngredient15}" +
                            $"\nMeasure: {drink.strMeasure1}  { drink.strMeasure2}  { drink.strMeasure3}  { drink.strMeasure4}" +
                            $"  { drink.strMeasure5}  { drink.strMeasure6}  { drink.strMeasure7}  { drink.strMeasure8}" +
                            $"  {drink.strMeasure9}  { drink.strMeasure10}  { drink.strMeasure11}  { drink.strMeasure12}" +
                            $"  { drink.strMeasure13}  { drink.strMeasure14}  { drink.strMeasure15}\nImage: {drink.strDrinkThumb}");

                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Open /start");
                return;
            }

            if (callbackQuery.Data.StartsWith("searchbyingredient"))
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Enter name of ingredient:", replyMarkup: new ForceReplyMarkup { Selective = true });
                return;
            }

            if (callbackQuery.Data.StartsWith("getcocktailfromdb"))
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Enter name of cocktail in db:", replyMarkup: new ForceReplyMarkup { Selective = true });
                return;
            }

            if (callbackQuery.Data.StartsWith("addtodb"))
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Enter name of cocktail to add in db:", replyMarkup: new ForceReplyMarkup { Selective = true });
                return;
            }

            if (callbackQuery.Data.StartsWith("getallfromdb"))
            {
                var cocktail = await GetAll();
                List<Database> drinks = cocktail;

                if (drinks == null) await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "No cocktails in db :(");

                if (drinks != null)
                    foreach (var drink in drinks)
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Name: {drink.strDrink}\nImage: {drink.strDrinkThumb}");

                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Open /start");
                return;
            }

            if (callbackQuery.Data.StartsWith("deletefromdb"))
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Enter cocktail to delete:", replyMarkup: new ForceReplyMarkup { Selective = true });
                return;
            }

            if (callbackQuery.Data.StartsWith("updateintodb"))
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Enter cocktail to update:", replyMarkup: new ForceReplyMarkup { Selective = true });
                return;
            }


        }
    }
}
