namespace RVM.CineTrack.Infrastructure.Services;

using System.Text.Json.Serialization;

// --- Search ---

public record TmdbSearchResult<T>(
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("results")] List<T> Results,
    [property: JsonPropertyName("total_pages")] int TotalPages,
    [property: JsonPropertyName("total_results")] int TotalResults);

public record TmdbMultiSearchItem(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("media_type")] string MediaType,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("original_title")] string? OriginalTitle,
    [property: JsonPropertyName("original_name")] string? OriginalName,
    [property: JsonPropertyName("overview")] string? Overview,
    [property: JsonPropertyName("poster_path")] string? PosterPath,
    [property: JsonPropertyName("backdrop_path")] string? BackdropPath,
    [property: JsonPropertyName("release_date")] string? ReleaseDate,
    [property: JsonPropertyName("first_air_date")] string? FirstAirDate,
    [property: JsonPropertyName("vote_average")] double VoteAverage,
    [property: JsonPropertyName("vote_count")] int VoteCount,
    [property: JsonPropertyName("genre_ids")] List<int>? GenreIds);

// --- Movie Details ---

public record TmdbMovieDetail(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("original_title")] string OriginalTitle,
    [property: JsonPropertyName("overview")] string? Overview,
    [property: JsonPropertyName("poster_path")] string? PosterPath,
    [property: JsonPropertyName("backdrop_path")] string? BackdropPath,
    [property: JsonPropertyName("release_date")] string? ReleaseDate,
    [property: JsonPropertyName("vote_average")] double VoteAverage,
    [property: JsonPropertyName("vote_count")] int VoteCount,
    [property: JsonPropertyName("runtime")] int? Runtime,
    [property: JsonPropertyName("genres")] List<TmdbGenre>? Genres,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("credits")] TmdbCredits? Credits);

// --- TV Details ---

public record TmdbTvDetail(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("original_name")] string OriginalName,
    [property: JsonPropertyName("overview")] string? Overview,
    [property: JsonPropertyName("poster_path")] string? PosterPath,
    [property: JsonPropertyName("backdrop_path")] string? BackdropPath,
    [property: JsonPropertyName("first_air_date")] string? FirstAirDate,
    [property: JsonPropertyName("vote_average")] double VoteAverage,
    [property: JsonPropertyName("vote_count")] int VoteCount,
    [property: JsonPropertyName("episode_run_time")] List<int>? EpisodeRunTime,
    [property: JsonPropertyName("genres")] List<TmdbGenre>? Genres,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("number_of_seasons")] int NumberOfSeasons,
    [property: JsonPropertyName("number_of_episodes")] int NumberOfEpisodes,
    [property: JsonPropertyName("seasons")] List<TmdbSeasonSummary>? Seasons,
    [property: JsonPropertyName("credits")] TmdbCredits? Credits);

public record TmdbSeasonSummary(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("season_number")] int SeasonNumber,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("overview")] string? Overview,
    [property: JsonPropertyName("poster_path")] string? PosterPath,
    [property: JsonPropertyName("air_date")] string? AirDate,
    [property: JsonPropertyName("episode_count")] int EpisodeCount);

// --- Season Detail ---

public record TmdbSeasonDetail(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("season_number")] int SeasonNumber,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("overview")] string? Overview,
    [property: JsonPropertyName("poster_path")] string? PosterPath,
    [property: JsonPropertyName("air_date")] string? AirDate,
    [property: JsonPropertyName("episodes")] List<TmdbEpisode>? Episodes);

public record TmdbEpisode(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("episode_number")] int EpisodeNumber,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("overview")] string? Overview,
    [property: JsonPropertyName("still_path")] string? StillPath,
    [property: JsonPropertyName("air_date")] string? AirDate,
    [property: JsonPropertyName("runtime")] int? Runtime);

// --- Credits ---

public record TmdbCredits(
    [property: JsonPropertyName("cast")] List<TmdbCastMember>? Cast,
    [property: JsonPropertyName("crew")] List<TmdbCrewMember>? Crew);

public record TmdbCastMember(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("character")] string? Character,
    [property: JsonPropertyName("profile_path")] string? ProfilePath,
    [property: JsonPropertyName("order")] int Order);

public record TmdbCrewMember(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("department")] string? Department,
    [property: JsonPropertyName("job")] string? Job,
    [property: JsonPropertyName("profile_path")] string? ProfilePath);

// --- Watch Providers ---

public record TmdbWatchProviderResponse(
    [property: JsonPropertyName("results")] Dictionary<string, TmdbCountryProviders>? Results);

public record TmdbCountryProviders(
    [property: JsonPropertyName("link")] string? Link,
    [property: JsonPropertyName("flatrate")] List<TmdbProvider>? Flatrate,
    [property: JsonPropertyName("rent")] List<TmdbProvider>? Rent,
    [property: JsonPropertyName("buy")] List<TmdbProvider>? Buy);

public record TmdbProvider(
    [property: JsonPropertyName("provider_id")] int ProviderId,
    [property: JsonPropertyName("provider_name")] string ProviderName,
    [property: JsonPropertyName("logo_path")] string? LogoPath);

// --- Genre ---

public record TmdbGenre(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name);

public record TmdbGenreList(
    [property: JsonPropertyName("genres")] List<TmdbGenre> Genres);

// --- Trending / Upcoming ---

public record TmdbUpcomingResponse(
    [property: JsonPropertyName("results")] List<TmdbMultiSearchItem> Results,
    [property: JsonPropertyName("dates")] TmdbDateRange? Dates);

public record TmdbDateRange(
    [property: JsonPropertyName("minimum")] string Minimum,
    [property: JsonPropertyName("maximum")] string Maximum);
