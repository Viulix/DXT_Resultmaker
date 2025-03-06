
public class Replay
{
    public string id { get; set; }
    public string link { get; set; }
    public DateTime created { get; set; }
    public Uploader uploader { get; set; }
    public string status { get; set; }
    public string rocket_league_id { get; set; }
    public string match_guid { get; set; }
    public string title { get; set; }
    public string map_code { get; set; }
    public string match_type { get; set; }
    public int team_size { get; set; }
    public string playlist_id { get; set; }
    public int duration { get; set; }
    public bool overtime { get; set; }
    public int overtime_seconds { get; set; }
    public int season { get; set; }
    public string season_type { get; set; }
    public DateTime date { get; set; }
    public bool date_has_timezone { get; set; }
    public string visibility { get; set; }
    public Group[] groups { get; set; }
    public Blue blue { get; set; }
    public Orange orange { get; set; }
    public string playlist_name { get; set; }
    public string map_name { get; set; }
}

public class Uploader
{
    public string steam_id { get; set; }
    public string name { get; set; }
    public string profile_url { get; set; }
    public string avatar { get; set; }
}

public class Blue
{
    public string color { get; set; }
    public string name { get; set; }
    public Player[] players { get; set; }
    public Stats stats { get; set; }
}

public class Stats
{
    public Ball ball { get; set; }
    public Core core { get; set; }
    public Boost boost { get; set; }
    public Movement movement { get; set; }
    public Positioning positioning { get; set; }
    public Demo demo { get; set; }
}

public class Ball
{
    public float possession_time { get; set; }
    public float time_in_side { get; set; }
}

public class Core
{
    public int shots { get; set; }
    public int shots_against { get; set; }
    public int goals { get; set; }
    public int goals_against { get; set; }
    public int saves { get; set; }
    public int assists { get; set; }
    public int score { get; set; }
    public int shooting_percentage { get; set; }
}

public class Boost
{
    public int bpm { get; set; }
    public float bcpm { get; set; }
    public float avg_amount { get; set; }
    public int amount_collected { get; set; }
    public int amount_stolen { get; set; }
    public int amount_collected_big { get; set; }
    public int amount_stolen_big { get; set; }
    public int amount_collected_small { get; set; }
    public int amount_stolen_small { get; set; }
    public int count_collected_big { get; set; }
    public int count_stolen_big { get; set; }
    public int count_collected_small { get; set; }
    public int count_stolen_small { get; set; }
    public int amount_overfill { get; set; }
    public int amount_overfill_stolen { get; set; }
    public int amount_used_while_supersonic { get; set; }
    public float time_zero_boost { get; set; }
    public float time_full_boost { get; set; }
    public float time_boost_0_25 { get; set; }
    public float time_boost_25_50 { get; set; }
    public float time_boost_50_75 { get; set; }
    public float time_boost_75_100 { get; set; }
}

public class Movement
{
    public int total_distance { get; set; }
    public float time_supersonic_speed { get; set; }
    public float time_boost_speed { get; set; }
    public float time_slow_speed { get; set; }
    public float time_ground { get; set; }
    public float time_low_air { get; set; }
    public float time_high_air { get; set; }
    public float time_powerslide { get; set; }
    public int count_powerslide { get; set; }
}

public class Positioning
{
    public float time_defensive_third { get; set; }
    public float time_neutral_third { get; set; }
    public float time_offensive_third { get; set; }
    public float time_defensive_half { get; set; }
    public float time_offensive_half { get; set; }
    public float time_behind_ball { get; set; }
    public float time_infront_ball { get; set; }
}

public class Demo
{
    public int inflicted { get; set; }
    public int taken { get; set; }
}

public class Player
{
    public int start_time { get; set; }
    public float end_time { get; set; }
    public string name { get; set; }
    public Id id { get; set; }
    public int car_id { get; set; }
    public string car_name { get; set; }
    public Camera camera { get; set; }
    public float steering_sensitivity { get; set; }
    public PlayerStats stats { get; set; }
}

public class Id
{
    public string platform { get; set; }
    public string id { get; set; }
}

public class Camera
{
    public int fov { get; set; }
    public int height { get; set; }
    public int pitch { get; set; }
    public int distance { get; set; }
    public float stiffness { get; set; }
    public float swivel_speed { get; set; }
    public float transition_speed { get; set; }
}

public class PlayerStats
{
    public Core core { get; set; }
    public Boost boost { get; set; }
    public Movement movement { get; set; }
    public Positioning positioning { get; set; }
    public Demo demo { get; set; }
}



public class Orange
{
    public string color { get; set; }
    public string name { get; set; }
    public Player[] players { get; set; }
    public Stats stats { get; set; }
}

public class Group
{
    public string id { get; set; }
    public string name { get; set; }
    public string link { get; set; }
}
