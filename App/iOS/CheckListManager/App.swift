import Foundation

public struct App: Decodable {
    
    // 1
    public let name: String
    public let link: String
    
    public init?(json: JSON) {
        // 2
        guard let nameContainer: JSON = "im:name" <~~ json,
            let idContainer: JSON = "id" <~~ json
            else { return nil }
        
        guard let name: String = "label" <~~ nameContainer,
            link: String = "label" <~~ idContainer
            else { return nil }
        
        self.name = name
        self.link = link
    }
    
}